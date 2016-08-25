using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowActionTests
    {
        [Test]
        public void Can_combine_two_workflow_actions_using_plus_operator()
        {
            var workflowAction = WorkflowAction.FailWorkflow("reason", "detail") + WorkflowAction.CompleteWorkflow("result");

            var workflowDecisions = workflowAction.GetDecisions();

            Assert.That(workflowDecisions,Is.EquivalentTo(new WorkflowDecision []{new FailWorkflowDecision("reason","detail"),new CompleteWorkflowDecision("result")}));
        }
    }
}