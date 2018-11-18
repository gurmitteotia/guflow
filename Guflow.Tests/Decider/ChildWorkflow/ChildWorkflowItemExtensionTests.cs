// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowItemExtensionTests
    {
        private Mock<IChildWorkflowItem> _childWorkflowItem;
        private EventGraphBuilder _builder;
        private Identity _identity;
        private SwfIdentity _scheduleId;
        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _identity = Identity.New("name", "ver", "pos");
            _scheduleId = _identity.ScheduleId();
            _childWorkflowItem = new Mock<IChildWorkflowItem>();
        }

        [Test]
        public void Result_can_return_string_as_dynamic_type()
        {
            var result = "result1";
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            var workflowResult = _childWorkflowItem.Object.Result();

            Assert.That((string)workflowResult, Is.EqualTo(result));

        }

        [Test]
        public void Result_can_return_quoted_string_as_dynamic_type()
        {
            var result = "\"result1\"";
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            var lambdaResult = _childWorkflowItem.Object.Result();

            Assert.That((string)lambdaResult, Is.EqualTo(result));

        }
        [Test]
        public void Result_can_return_complex_activity_result_as_dynamic_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            var lambdaResult = _childWorkflowItem.Object.Result();

            Assert.That((int)lambdaResult.Id, Is.EqualTo(1));
            Assert.That((string)lambdaResult.Name, Is.EqualTo("test"));
        }

        [Test]
        public void Throws_exception_when_last_event_is_not_completed_event()
        {
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(FailedEvent("reason", "details"));

            Assert.Throws<InvalidOperationException>(() => _childWorkflowItem.Object.Result());
        }

        [Test]
        public void Result_can_return_complex_activity_result_as_complex_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            var lambdaResult = _childWorkflowItem.Object.Result<ResultType>();

            Assert.That(lambdaResult.Id, Is.EqualTo(1));
            Assert.That(lambdaResult.Name, Is.EqualTo("test"));
        }

        [Test]
        public void Result_throws_exception_when_casting_complex_type_to_primitive_type()
        {
            var result = new { Id = 1, Name = "test" }.ToAwsString();
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(result));

            Assert.Throws<InvalidCastException>(() => _childWorkflowItem.Object.Result<int>());
        }

        [Test]
        public void Has_completed()
        {
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent(""));
            Assert.IsTrue(_childWorkflowItem.Object.HasCompleted());

            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(FailedEvent("", ""));
            Assert.IsFalse(_childWorkflowItem.Object.HasCompleted());
        }

        [Test]
        public void Has_failed()
        {
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(FailedEvent("r", "d"));
            Assert.IsTrue(_childWorkflowItem.Object.HasFailed());

            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent("d"));
            Assert.IsFalse(_childWorkflowItem.Object.HasFailed());
        }

        [Test]
        public void Has_timedout()
        {
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(TimedoutEvent("d"));
            Assert.IsTrue(_childWorkflowItem.Object.HasTimedout());

            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent("d"));
            Assert.IsFalse(_childWorkflowItem.Object.HasTimedout());
        }

        [Test]
        public void Has_terminated()
        {
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(TerminatedEvent());
            Assert.IsTrue(_childWorkflowItem.Object.HasTerminated());

            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent("d"));
            Assert.IsFalse(_childWorkflowItem.Object.HasTerminated());
        }

        [Test]
        public void Has_cancelled()
        {
            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CancelledEvent("d"));
            Assert.IsTrue(_childWorkflowItem.Object.HasCancelled());

            _childWorkflowItem.Setup(a => a.LastEvent(false)).Returns(CompletedEvent("d"));
            Assert.IsFalse(_childWorkflowItem.Object.HasCancelled());
        }


        [Test]
        public void Null_argument_tests()
        {
            IChildWorkflowItem childWorkflowItem = null;
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.Result());
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.Result<int>());
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.HasCompleted());
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.HasFailed());
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.HasTimedout());
        }



        private ChildWorkflowCompletedEvent CompletedEvent(string result)
        {
            var graph = _builder.ChildWorkflowCompletedGraph(_scheduleId, "runid", "input", result);
            return new ChildWorkflowCompletedEvent(graph.First(), graph);
        }

        private ChildWorkflowFailedEvent FailedEvent(string reason, string details)
        {
            var graph = _builder.ChildWorkflowFailedEventGraph(_scheduleId, "runid", "input", reason, details);
            return new ChildWorkflowFailedEvent(graph.First(), graph);
        }
        private ChildWorkflowTimedoutEvent TimedoutEvent(string timeoutType)
        {
            var graph = _builder.ChildWorkflowTimedoutEventGraph(_scheduleId, "runid", "input", timeoutType);
            return new ChildWorkflowTimedoutEvent(graph.First(), graph);
        }
        private ChildWorkflowTerminatedEvent TerminatedEvent()
        {
            var graph = _builder.ChildWorkflowTerminatedEventGraph(_scheduleId, "runid", "input");
            return new ChildWorkflowTerminatedEvent(graph.First(), graph);
        }
        private ChildWorkflowCancelledEvent CancelledEvent(string details)
        {
            var graph = _builder.ChildWorkflowCancelledEventGraph(_scheduleId, "runid", "input", details);
            return new ChildWorkflowCancelledEvent(graph.First(), graph);
        }

        private class ResultType
        {
            public int Id;
            public string Name;
        }
    }
}