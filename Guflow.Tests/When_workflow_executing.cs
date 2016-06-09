using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class When_workflow_executing_and_no_decisions_are_generated
    {
        private Mock<IWorkflowHistoryEvents> _workflowHistoryEvents;
        private Workflow _workflow;
        [SetUp]
        public void Setup()
        {
            _workflowHistoryEvents = new Mock<IWorkflowHistoryEvents>();    
            _workflow = new EmptyWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(_workflow)).Returns(new WorkflowDecision[0]);
        }

        [Test]
        public void Return_empty_decisions_when_workflow_is_active()
        {
            _workflowHistoryEvents.Setup(w => w.IsActive()).Returns(true);

            var workflowDecisions = _workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions,Is.Empty);
        }

        [Test]
        public void Return_complete_workflow_decision_when_workflow_is_not_active()
        {
            _workflowHistoryEvents.Setup(w => w.IsActive()).Returns(false);

            var workflowDecisions = _workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EquivalentTo(new []{new CompleteWorkflowDecision("result")}));
        }
       
    }
}