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

        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
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
        public void Can_return_custom_workflow_action()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("name", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new WorkflowWithCustomActionOnSignal(expectedAction));

            Assert.That(workflowAction, Is.EqualTo(expectedAction));
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