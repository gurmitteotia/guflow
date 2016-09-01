using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    public class WorkflowCancelRequestFailedEventTests
    {
        private WorkflowCancelRequestFailedEvent _cancelRequestFailedEvent;

        [SetUp]
        public void Setup()
        {
            _cancelRequestFailedEvent = new WorkflowCancelRequestFailedEvent(HistoryEventFactory.CreateWorkflowCancelRequestFailedEvent("cause"));
        }

        [Test]
        public void Populate_properties_from_swf_event()
        {
            Assert.That(_cancelRequestFailedEvent.Cause,Is.EqualTo("cause"));
        }

        [Test]
        public void By_default_returns_fail_workflow_action_when_interpreted()
        {
            var workflowAction = _cancelRequestFailedEvent.Interpret(new EmptyWorkflow());

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow("FAILED_TO_SEND_CANCEL_REQUEST","cause")));
        }

        [Test]
        public void Can_return_custom_workflow_action_when_interpreted()
        {
            var expectedWorkflowAction = new Mock<WorkflowAction>().Object;

            var workflowAction = _cancelRequestFailedEvent.Interpret(new WorkflowToReturnCustomAction(expectedWorkflowAction));

            Assert.That(workflowAction,Is.EqualTo(expectedWorkflowAction));
        }

        private class WorkflowToReturnCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowToReturnCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.CancelRequestFailed)]
            public WorkflowAction OnCancelRequestFailed(WorkflowCancelRequestFailedEvent @event)
            {
                return _workflowAction;
            }
        }
    }
}