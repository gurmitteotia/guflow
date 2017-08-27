using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowCancellationRequestedEventTests
    {
        private WorkflowCancellationRequestedEvent _cancellationRequestedEvent;
       
        [SetUp]
        public void Setup()
        {
            var cancellationRequestedEvent = HistoryEventFactory.CreateWorkflowCancellationRequestedEvent("cause","runid","id");
            _cancellationRequestedEvent = new WorkflowCancellationRequestedEvent(cancellationRequestedEvent);
        }

        [Test]
        public void Populates_properties_from_event_attributes()
        {
            Assert.That(_cancellationRequestedEvent.Cause,Is.EqualTo("cause"));
            Assert.That(_cancellationRequestedEvent.ExternalWorkflowRunid, Is.EqualTo("runid"));
            Assert.That(_cancellationRequestedEvent.ExternalWorkflowId, Is.EqualTo("id"));
        }
        [Test]
        public void Does_not_populates_external_workflow_properties_when_not_generated_by_external_workflow()
        {
            var cancellationRequestedEvent = new WorkflowCancellationRequestedEvent(HistoryEventFactory.CreateWorkflowCancellationRequestedEvent("cause"));
            Assert.That(cancellationRequestedEvent.Cause, Is.EqualTo("cause"));
            Assert.That(cancellationRequestedEvent.ExternalWorkflowRunid, Is.Null);
            Assert.That(cancellationRequestedEvent.ExternalWorkflowId, Is.Null);
        }
        [Test]
        public void By_default_return_cancel_workflow_action()
        {
            var decisions = _cancellationRequestedEvent.Interpret(new EmptyWorkflow()).GetDecisions();

            Assert.That(decisions,Is.EqualTo(new []{new CancelWorkflowDecision("cause")}));
        }
        [Test]
        public void Can_return_custom_workflow_action()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            var workflowAction = _cancellationRequestedEvent.Interpret(new WorkflowToReturnCustomAction(expectedAction));

            Assert.That(workflowAction, Is.EqualTo(expectedAction));
        }

        private class WorkflowToReturnCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowToReturnCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.CancelRequest)]
            protected WorkflowAction OnCancellationRequested(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent)
            {
                return _workflowAction;
            }
        }
    }
}