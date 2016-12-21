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

        [SetUp]
        public void Setup()
        {
            var signaledEvent = HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input", "externalWorkflowRunid", "externalWorkflowRunid");
            _workflowSignaledEvent = new WorkflowSignaledEvent(signaledEvent);
        }
        [Test]
        public void Populates_properties_from_signaled_event()
        {
            Assert.That(_workflowSignaledEvent.SignalName,Is.EqualTo("name"));
            Assert.That(_workflowSignaledEvent.Input, Is.EqualTo("input"));
            Assert.That(_workflowSignaledEvent.ExternalWorkflowRunid, Is.EqualTo("externalWorkflowRunid"));
            Assert.That(_workflowSignaledEvent.ExternalWorkflowId, Is.EqualTo("externalWorkflowRunid"));
        }
        [Test]
        public void Does_not_populate_external_workflow_properties_when_not_signed_by_external_workflow()
        {
            var workflowSignaledEvent = new WorkflowSignaledEvent(HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input"));
            Assert.That(workflowSignaledEvent.SignalName, Is.EqualTo("name"));
            Assert.That(workflowSignaledEvent.Input, Is.EqualTo("input"));
            Assert.That(workflowSignaledEvent.ExternalWorkflowRunid, Is.Null);
            Assert.That(workflowSignaledEvent.ExternalWorkflowId, Is.Null);
        }
        [Test]
        public void By_default_returns_workflow_ignore_action()
        {
            var workflowSignaledEvent = new WorkflowSignaledEvent(HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new EmptyWorkflow());

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.Ignore));
        }
        [Test]
        public void Can_return_custom_workflow_action()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowSignaledEvent = new WorkflowSignaledEvent(HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input"));

            var workflowAction = workflowSignaledEvent.Interpret(new WorkflowWithCustomActionOnSignal(expectedAction));

            Assert.That(workflowAction, Is.EqualTo(expectedAction));
        }
        [Test]
        public void Workflow_can_reply_to_signal_event()
        {
            var workflowSignaledEvent = new WorkflowSignaledEvent(HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input","runid","wid"));

            var workflowAction = workflowSignaledEvent.Interpret(new WorkflowToReplyToSignalEvent("newSignal","newInput"));

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Signal("newSignal","newInput",workflowSignaledEvent.ExternalWorkflowId,workflowSignaledEvent.ExternalWorkflowRunid)));
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