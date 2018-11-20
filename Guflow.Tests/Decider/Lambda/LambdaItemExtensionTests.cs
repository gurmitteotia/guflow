// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaItemExtensionTests
    {
        private Mock<ILambdaItem> _lambdaItem;
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _lambdaItem = new Mock<ILambdaItem>();
        }

        [Test]
        public void Result_can_return_string_as_dynamic_type()
        {
            var result = "result1";
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            var lambdaResult = _lambdaItem.Object.Result();

            Assert.That((string)lambdaResult, Is.EqualTo(result));

        }

        [Test]
        public void Result_can_return_quoted_string_as_dynamic_type()
        {
            var result = "\"result1\"";
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            var lambdaResult = _lambdaItem.Object.Result();

            Assert.That((string)lambdaResult, Is.EqualTo(result));

        }
        [Test]
        public void Result_can_return_complex_activity_result_as_dynamic_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            var lambdaResult = _lambdaItem.Object.Result();

            Assert.That((int)lambdaResult.Id, Is.EqualTo(1));
            Assert.That((string)lambdaResult.Name, Is.EqualTo("test"));
        }

        [Test]
        public void Throws_exception_when_last_event_is_not_completed_event()
        {
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(FailedEvent("reason", "details"));

            Assert.Throws<InvalidOperationException>(() => _lambdaItem.Object.Result());
        }

        [Test]
        public void Result_can_return_complex_activity_result_as_complex_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            var lambdaResult = _lambdaItem.Object.Result<ResultType>();

            Assert.That(lambdaResult.Id, Is.EqualTo(1));
            Assert.That(lambdaResult.Name, Is.EqualTo("test"));
        }

        [Test]
        public void Result_throws_exception_when_casting_complex_type_to_primitive_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            Assert.Throws<InvalidCastException>(() => _lambdaItem.Object.Result<int>());
        }

        [Test]
        public void Has_completed()
        {
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(""));
            Assert.IsTrue(_lambdaItem.Object.HasCompleted());

            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(FailedEvent("",""));
            Assert.IsFalse(_lambdaItem.Object.HasCompleted());
        }

        [Test]
        public void Has_failed()
        {
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(FailedEvent("r", "d"));
            Assert.IsTrue(_lambdaItem.Object.HasFailed());

            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent("d"));
            Assert.IsFalse(_lambdaItem.Object.HasFailed());
        }
        
        [Test]
        public void Has_timedout()
        {
            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(TimedoutEvent("d"));
            Assert.IsTrue(_lambdaItem.Object.HasTimedout());

            _lambdaItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent("d"));
            Assert.IsFalse(_lambdaItem.Object.HasTimedout());
        }
      

        [Test]
        public void Null_argument_tests()
        {
            ILambdaItem lambdaItem = null;
            Assert.Throws<ArgumentNullException>(() => lambdaItem.Result());
            Assert.Throws<ArgumentNullException>(() => lambdaItem.Result<int>());
            Assert.Throws<ArgumentNullException>(() => lambdaItem.HasCompleted());
            Assert.Throws<ArgumentNullException>(() => lambdaItem.HasFailed());
            Assert.Throws<ArgumentNullException>(() => lambdaItem.HasTimedout());
        }



        private LambdaCompletedEvent CompletedEvent(string result)
        {
            var graph = _builder.LambdaCompletedEventGraph(Identity.Lambda("lambda").ScheduleId(), "input", result);
            return new LambdaCompletedEvent(graph.First(), graph);
        }

        private LambdaFailedEvent FailedEvent(string reason, string details)
        {
            var graph = _builder.LambdaFailedEventGraph(Identity.Lambda("lambda").ScheduleId(), "input", reason,details);
            return new LambdaFailedEvent(graph.First(), graph);
        }
        private LambdaTimedoutEvent TimedoutEvent(string timeoutType)
        {
            var graph = _builder.LamdbaTimedoutEventGraph(Identity.Lambda("lambda").ScheduleId(), "input", timeoutType);
            return new LambdaTimedoutEvent(graph.First(), graph);
        }


        private class ResultType
        {
            public int Id;
            public string Name;
        }
    }
}