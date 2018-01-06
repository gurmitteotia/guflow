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
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent(result));

            var activityResult = _activityItem.Object.Result();

            Assert.That((int)activityResult.Id, Is.EqualTo(1));
            Assert.That((string)activityResult.Name, Is.EqualTo("test"));
        }

        [Test]
        public void Result_can_return_string_activity_result_as_dynamic_type()
        {
            var result = "test";
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent(result));

            var activityResult = _activityItem.Object.Result();

            Assert.That((string)activityResult, Is.EqualTo("test"));
        }

        [Test]
        public void Result_throws_exception_when_last_event_is_not_activity_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(FailedEvent("r","d"));

            Assert.Throws<InvalidOperationException>(() => _activityItem.Object.Result());
        }

        [Test]
        public void Result_can_cast_complex_activity_result_to_complex_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent(result));

            var activityResult = _activityItem.Object.Result<ResultType>();

            Assert.That(activityResult.Id, Is.EqualTo(1));
            Assert.That(activityResult.Name, Is.EqualTo("test"));
        }

        [Test]
        public void Result_can_cast_int_activity_result_to_int_type()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent("1"));

            var activityResult = _activityItem.Object.Result<int>();

            Assert.That(activityResult, Is.EqualTo(1));
        }

        [Test]
        public void Result_throws_exception_when_casting_complex_type_to_primitive_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent(result));

            Assert.Throws<InvalidCastException>(()=>_activityItem.Object.Result<int>());
        }

        [Test]
        public void Has_completed_returns_true_when_last_event_is_activity_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent(""));
            Assert.IsTrue(_activityItem.Object.HasCompleted());
        }

        [Test]
        public void Has_completed_returns_false_when_last_event_is_not_activity_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(StartedEvent());
            Assert.IsFalse(_activityItem.Object.HasCompleted());
        }

        [Test]
        public void Has_failed_returns_true_when_last_event_is_activity_failed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(FailedEvent("r", "d"));
            Assert.IsTrue(_activityItem.Object.HasFailed());
        }
        [Test]
        public void Has_failed_returns_false_when_last_event_is_not_activity_failed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent("r"));
            Assert.IsFalse(_activityItem.Object.HasFailed());
        }

        [Test]
        public void Has_cancelled_returns_true_when_last_event_is_activity_cancelled_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CancelledEvent("d"));
            Assert.IsTrue(_activityItem.Object.HasCancelled());
        }
        [Test]
        public void Has_cancelled_returns_false_when_last_event_is_not_activity_cancelled_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent("r"));
            Assert.IsFalse(_activityItem.Object.HasCancelled());
        }

        [Test]
        public void Has_timedout_returns_true_when_last_event_is_activity_timedout_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(TimedoutEvent("d", "d1"));
            Assert.IsTrue(_activityItem.Object.HasTimedout());
        }
        [Test]
        public void Has_timedout_returns_false_when_last_event_is_not_activity_timedout_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent("r"));
            Assert.IsFalse(_activityItem.Object.HasTimedout());
        }

        [Test]
        public void Null_argument_tests()
        {
            IActivityItem activityItem = null;
            Assert.Throws<ArgumentNullException>(() => activityItem.Result());
            Assert.Throws<ArgumentNullException>(() => activityItem.Result<int>());
            Assert.Throws<ArgumentNullException>(() => activityItem.HasCompleted());
        }

        [Test]
        public void Last_failed_event_is_not_null_when_the_last_event_of_activity_is_failed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(FailedEvent("reason", "details"));
            var lastFailedEvent = _activityItem.Object.LastFailedEvent();

            Assert.IsNotNull(lastFailedEvent);
            Assert.That(lastFailedEvent.Reason, Is.EqualTo("reason"));
            Assert.That(lastFailedEvent.Details, Is.EqualTo("details"));
        }
        [Test]
        public void Last_failed_event_is_null_when_the_latest_event_is_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent("res"));
            var lastFailedEvent = _activityItem.Object.LastFailedEvent();

            Assert.IsNull(lastFailedEvent);
        }

        [Test]
        public void Last_timed_event_is_not_null_when_the_last_event_of_activity_is_timedout_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(TimedoutEvent("reason", "details"));
            var lastTimedoutEvent = _activityItem.Object.LastTimedoutEvent();

            Assert.IsNotNull(lastTimedoutEvent);
            Assert.That(lastTimedoutEvent.TimeoutType, Is.EqualTo("reason"));
            Assert.That(lastTimedoutEvent.Details, Is.EqualTo("details"));
        }
        [Test]
        public void Last_timedout_event_is_null_when_the_latest_event_is_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent("res"));
            var lastTimedoutEvent = _activityItem.Object.LastTimedoutEvent();

            Assert.IsNull(lastTimedoutEvent);
        }

        [Test]
        public void Last_cancelled_event_is_not_null_when_the_last_event_of_activity_is_cancelled_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CancelledEvent("details"));
            var lastCancelledEvent = _activityItem.Object.LastCancelledEvent();

            Assert.IsNotNull(lastCancelledEvent);
            Assert.That(lastCancelledEvent.Details, Is.EqualTo("details"));
        }
        [Test]
        public void Last_cancelled_event_is_null_when_the_latest_event_is_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent("res"));
            var lastCancelledEvent = _activityItem.Object.LastCancelledEvent();

            Assert.IsNull(lastCancelledEvent);
        }

        [Test]
        public void Last_completed_event_is_not_null_when_the_last_event_of_activity_is_completed_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CompletedEvent("details"));
            var lastCompletedEvent = _activityItem.Object.LastCompletedEvent();

            Assert.IsNotNull(lastCompletedEvent);
            Assert.That(lastCompletedEvent.Result, Is.EqualTo("details"));
        }
        [Test]
        public void Last_completed_event_is_null_when_the_latest_event_is_cancelled_event()
        {
            _activityItem.SetupGet(a => a.LastEvent).Returns(CancelledEvent("res"));
            var lastCompletedEvent = _activityItem.Object.LastCompletedEvent();

            Assert.IsNull(lastCompletedEvent);
        }

        private ActivityCompletedEvent CompletedEvent(string result)
        {
            var eventGraph = _builder.ActivityCompletedGraph(Identity.New("a1", "v1"), "id",
                result, "input");
            return new ActivityCompletedEvent(eventGraph.First(), eventGraph);
        }

        private ActivityFailedEvent FailedEvent(string reason, string detail)
        {
            var eventGraph = _builder.ActivityFailedGraph(Identity.New("a","v"),"id",reason, detail);
            return new ActivityFailedEvent(eventGraph.First(), eventGraph);
        }

        private ActivityTimedoutEvent TimedoutEvent(string timeoutType, string details)
        {
            var eventGraph = _builder.ActivityTimedoutGraph(Identity.New("a", "v"), "id", timeoutType, details);
            return new ActivityTimedoutEvent(eventGraph.First(), eventGraph);
        }

        private ActivityCancelledEvent CancelledEvent(string details)
        {
            var eventGraph = _builder.ActivityCancelledGraph(Identity.New("a", "v"), "id", details);
            return new ActivityCancelledEvent(eventGraph.First(), eventGraph);
        }

        private ActivityStartedEvent StartedEvent()
        {
            var eventGraph = _builder.ActivityStartedGraph(Identity.New("a", "v"), "id");
            return new ActivityStartedEvent(eventGraph.First(), eventGraph);
        }

        private class ResultType
        {
            public int Id;
            public string Name;
        }
    }
}