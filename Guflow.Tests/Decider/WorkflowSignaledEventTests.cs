// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowSignaledEventTests
    {
        private WorkflowSignaledEvent _workflowSignaledEvent;

        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var signaledEvent = _builder.WorkflowSignaledEvent("name", "input", "externalWorkflowRunid", "externalWorkflowRunid");
            _workflowSignaledEvent = new WorkflowSignaledEvent(signaledEvent);
        }
        [Test]
        public void Populates_properties_from_signaled_event()
        {
            Assert.That(_workflowSignaledEvent.SignalName,Is.EqualTo("name"));
            Assert.That(_workflowSignaledEvent.Input, Is.EqualTo("input"));
            Assert.That(_workflowSignaledEvent.ExternalWorkflowRunid, Is.EqualTo("externalWorkflowRunid"));
            Assert.That(_workflowSignaledEvent.ExternalWorkflowId, Is.EqualTo("externalWorkflowRunid"));
            Assert.IsTrue(_workflowSignaledEvent.IsSentByWorkflow);

        }
        [Test]
        public void Does_not_populate_external_workflow_properties_when_not_signed_by_external_workflow()
        {
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("name", "input"));
            Assert.That(workflowSignaledEvent.SignalName, Is.EqualTo("name"));
            Assert.That(workflowSignaledEvent.Input, Is.EqualTo("input"));
            Assert.That(workflowSignaledEvent.ExternalWorkflowRunid, Is.Null);
            Assert.That(workflowSignaledEvent.ExternalWorkflowId, Is.Null);
            Assert.IsFalse(workflowSignaledEvent.IsSentByWorkflow);
        }
        [Test]
        public void By_default_returns_workflow_ignore_action()
        {
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("name", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new EmptyWorkflow());

            Assert.That(workflowAction.Decisions(),Is.Empty);
        }
        [Test]
        public void Can_return_custom_workflow_action_using_generic_handler()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("name", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new WorkflowWithCustomActionOnSignal(expectedAction));

            Assert.That(workflowAction, Is.EqualTo(expectedAction));
        }

        [Test]
        public void Signal_can_be_handled_using_matching_name_by_a_signal_method()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("signal1", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new WorkflowWithSignalAttribute(expectedAction));

            Assert.That(workflowAction, Is.EqualTo(expectedAction));
        }

        [Test]
        public void Signal_can_be_handled_by_signal_method_by_giving_a_signal_name_property()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("signal1", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new WorkflowWithSignalName(expectedAction));

            Assert.That(workflowAction, Is.EqualTo(expectedAction));
        }

        [Test]
        public void Signal_method_with_a_signal_name_takes_priority_over_matching_signal_method()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("signal1", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new WorkflowWithMatchingSignalMethodAndName(expectedAction));

            Assert.That(workflowAction, Is.EqualTo(expectedAction));
        }

        [Test]
        public void Fall_back_to_generic_workflow_event_handler_when_signal_name_does_not_match()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("signal1", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new WorkflowWithNotMatchingSignalMethod(expectedAction));

            Assert.That(workflowAction, Is.EqualTo(expectedAction));
        }

        [Test]
        public void Throws_exception_when_two_signal_method_exists_with_same_name()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("signal1", "input"));

            Assert.Throws<AmbiguousWorkflowMethodException>(()=> workflowSignaledEvent.Interpret(new WorkflowWithTwoSignalMethodsForSameSignal(expectedAction)));
        }
        [Test]
        public void Throws_exception_when_two_signal_method_exists_for_same_signal_name()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("signal1", "input"));

            Assert.Throws<AmbiguousWorkflowMethodException>(() => workflowSignaledEvent.Interpret(new WorkflowWithTwoSignalMethodsForSameSignalName(expectedAction)));
        }

        [Test]
        public void Workflow_can_reply_to_signal_event()
        {
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("name", "input","runid","wid"));

            var decisions = workflowSignaledEvent.Interpret(new WorkflowToReplyToSignalEvent("newSignal","newInput")).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new SignalWorkflowDecision("newSignal","newInput",workflowSignaledEvent.ExternalWorkflowId,workflowSignaledEvent.ExternalWorkflowRunid)}));
        }

        [Test]
        public void Reply_to_signal_event_not_sent_by_a_workflow_generate_exception()
        {
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("name", "input"));

            Assert.Throws<SignalException>(()=> workflowSignaledEvent.Interpret(new WorkflowToReplyToSignalEvent("newSignal", "newInput")));
        }

        private class WorkflowWithCustomActionOnSignal : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowWithCustomActionOnSignal(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.Signal)]
            protected WorkflowAction OnSignal(WorkflowSignaledEvent workflowSignalEvent)
            {
                return _workflowAction;
            }
        }

        private class WorkflowWithSignalAttribute : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowWithSignalAttribute(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [Signal]
            protected WorkflowAction Signal1(WorkflowSignaledEvent workflowSignalEvent)
            {
                return _workflowAction;
            }
        }

        private class WorkflowWithSignalName : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowWithSignalName(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [Signal(Name= "Signal1")]
            protected WorkflowAction OnSignal(WorkflowSignaledEvent workflowSignalEvent)
            {
                return _workflowAction;
            }
        }

        private class WorkflowWithMatchingSignalMethodAndName : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowWithMatchingSignalMethodAndName(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [Signal(Name = "Signal1")]
            protected WorkflowAction OnSignal(WorkflowSignaledEvent workflowSignalEvent)
            {
                return _workflowAction;
            }
            [Signal]
            protected WorkflowAction Signal1(string details)
            {
                return WorkflowAction.Empty;
            }
        }
        private class WorkflowWithTwoSignalMethodsForSameSignal : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowWithTwoSignalMethodsForSameSignal(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [Signal]
            protected WorkflowAction Signal1(WorkflowSignaledEvent workflowSignalEvent)
            {
                return _workflowAction;
            }
            [Signal]
            protected WorkflowAction Signal1(string details)
            {
                return WorkflowAction.Empty;
            }
        }

        private class WorkflowWithTwoSignalMethodsForSameSignalName : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowWithTwoSignalMethodsForSameSignalName(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [Signal(Name = "signal1")]
            protected WorkflowAction OnSignal(WorkflowSignaledEvent workflowSignalEvent)
            {
                return _workflowAction;
            }
            [Signal(Name = "signal1")]
            protected WorkflowAction OnSignal(string details)
            {
                return WorkflowAction.Empty;
            }
        }
        private class WorkflowWithNotMatchingSignalMethod : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowWithNotMatchingSignalMethod(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [Signal(Name = "SignalNotMatching")]
            protected WorkflowAction OnSignal(WorkflowSignaledEvent workflowSignalEvent)
            {
                return Ignore;
            }
            [WorkflowEvent(EventName.Signal)]
            protected WorkflowAction Signal1(string details)
            {
                return _workflowAction;
            }
        }

        private class WorkflowToReplyToSignalEvent : Workflow
        {
            private readonly string _signalName;
            private readonly string _input;

            public WorkflowToReplyToSignalEvent(string signalName, string input)
            {
                _signalName = signalName;
                _input = input;
            }

            [WorkflowEvent(EventName.Signal)]
            protected WorkflowAction OnSignal(WorkflowSignaledEvent workflowSignalEvent)
            {
                return Signal(_signalName, _input).ReplyTo(workflowSignalEvent);
            }
        }
    }
}