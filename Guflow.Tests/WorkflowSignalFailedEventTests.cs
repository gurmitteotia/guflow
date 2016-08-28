using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowSignalFailedEventTests
    {
        private WorkflowSignalFailedEvent _workflowSignaledEvent;
        [SetUp]
        public void Setup()
        {
            _workflowSignaledEvent = new WorkflowSignalFailedEvent(HistoryEventFactory.CreateWorkflowSignalFailedEvent("cause","wid","rid"));
        }
        [Test]
        public void Populate_properties_from_event()
        {
            Assert.That(_workflowSignaledEvent.Cause,Is.EqualTo("cause"));
            Assert.That(_workflowSignaledEvent.WorkflowId,Is.EqualTo("wid"));
            Assert.That(_workflowSignaledEvent.RunId,Is.EqualTo("rid"));
        }

        [Test]
        public void By_default_Returns_fail_workflow_action_when_interpreted()
        {
            var workflowAction = _workflowSignaledEvent.Interpret(new EmptyWorkflow());

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow("FAILED_TO_SIGNAL_WORKFLOW","cause")));
        }

        [Test]
        public void Can_return_custom_action_when_interpreted()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;

            var actualAction = _workflowSignaledEvent.Interpret(new WorkflowToReturnCustomAction(expectedAction));

            Assert.That(actualAction,Is.EqualTo(expectedAction));
        }

        private class WorkflowToReturnCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;

            public WorkflowToReturnCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [SignalFailed]
            protected WorkflowAction OnFailToSignalWorkflow(WorkflowSignalFailedEvent workflowSignalFailedEvent)
            {
                return _workflowAction;
            }
        }
    }
}