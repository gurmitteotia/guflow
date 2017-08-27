using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowCancellationFailedEventTests
    {
        private WorkflowCancellationFailedEvent _cancellationFailedEvent;

        [SetUp]
        public void Setup()
        {
            _cancellationFailedEvent = new WorkflowCancellationFailedEvent(HistoryEventFactory.CreateWorkflowCancellationFailedEvent("cause"));
        }

        [Test]
        public void Populates_properties_from_swf_event_attributes()
        {
            Assert.That(_cancellationFailedEvent.Cause,Is.EqualTo("cause"));
        }

        [Test]
        public void By_default_returns_workflow_failed_decision()
        {
            var decisions = _cancellationFailedEvent.Interpret(new EmptyWorkflow()).GetDecisions();

            Assert.That(decisions,Is.EqualTo(new []{new FailWorkflowDecision("FAILED_TO_CANCEL_WORKFLOW","cause")}));
        }

        [Test]
        public void Can_return_custom_workflow_action()
        {
            var expectedWorkflowAction = new Mock<WorkflowAction>().Object;

            var workflowAction = _cancellationFailedEvent.Interpret(new WorkflowToReturnCustomAction(expectedWorkflowAction));

            Assert.That(workflowAction, Is.EqualTo(expectedWorkflowAction));
        }

        private class WorkflowToReturnCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;

            public WorkflowToReturnCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.CancellationFailed)]
            public WorkflowAction OnCancellationFailed(WorkflowCancellationFailedEvent @event)
            {
                return _workflowAction;
            }
        }
    }
}