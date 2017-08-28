using System;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityItemExtensionTests
    {
        private Mock<IActivityItem> _activityItem;
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            _activityItem = new Mock<IActivityItem>();
        }

        [Test]
        public void Result_can_return_complex_activity_result_as_dynamic_type()
        {
            var result = new {Id = 1, Name = "test"}.ToAwsString();
            _activityItem.SetupGet(a => a.LastEvent).Returns(CreateCompltedEvent(result));

            var activityResult = _activityItem.Object.Result();

            Assert.That((int)activityResult.Id, Is.EqualTo(1));
            Assert.That((string)activityResult.Name, Is.EqualTo("test"));
        }

        [Test]
        public void Result_can_return_string_activity_result_as_dynamic_type()
        {
            var result = "test";
            _activityItem.SetupGet(a => a.LastEvent).Returns(CreateCompltedEvent(result));

            var activityResult = _activityItem.Object.Result();

            Assert.That((string)activityResult, Is.EqualTo("test"));
        }

        [Test]
        public void Result_throws_exception_when_last_event_is_not_activity_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(WorkflowItemEvent.NotFound);

            Assert.Throws<InvalidOperationException>(() => _activityItem.Object.Result());
        }

        [Test]
        public void Result_can_cast_complex_activity_result_to_complex_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _activityItem.SetupGet(a => a.LastEvent).Returns(CreateCompltedEvent(result));

            var activityResult = _activityItem.Object.Result<ResultType>();

            Assert.That(activityResult.Id, Is.EqualTo(1));
            Assert.That(activityResult.Name, Is.EqualTo("test"));
        }

        [Test]
        public void Result_can_cast_int_activity_result_to_int_type()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CreateCompltedEvent("1"));

            var activityResult = _activityItem.Object.Result<int>();

            Assert.That(activityResult, Is.EqualTo(1));
        }

        [Test]
        public void Result_throws_exception_when_casting_complex_type_to_primitive_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _activityItem.SetupGet(a => a.LastEvent).Returns(CreateCompltedEvent(result));

            Assert.Throws<InvalidCastException>(()=>_activityItem.Object.Result<int>());
        }

        [Test]
        public void Has_completed_returns_true_when_last_event_is_activity_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CreateCompltedEvent(""));
            Assert.IsTrue(_activityItem.Object.HasCompleted());
        }

        [Test]
        public void Has_completed_returns_false_when_last_event_is_not_activity_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(WorkflowItemEvent.NotFound);
            Assert.IsFalse(_activityItem.Object.HasCompleted());
        }

        [Test]
        public void Null_argument_tests()
        {
            IActivityItem activityItem = null;
            Assert.Throws<ArgumentNullException>(() => activityItem.Result());
            Assert.Throws<ArgumentNullException>(() => activityItem.Result<int>());
            Assert.Throws<ArgumentNullException>(() => activityItem.HasCompleted());
        }

        private ActivityCompletedEvent CreateCompltedEvent(string result)
        {
            var eventGraph = _builder.ActivityCompletedGraph(Identity.New("a1", "v1"), "id",
                result, "input");
            return new ActivityCompletedEvent(eventGraph.First(), eventGraph);
        }

        private class ResultType
        {
            public int Id;
            public string Name;
        }
    }
}