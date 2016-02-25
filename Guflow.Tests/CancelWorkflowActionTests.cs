using NUnit.Framework;
using System.Linq;

namespace Guflow.Tests
{
    [TestFixture]
    public class CancelWorkflowActionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(WorkflowAction.CancelWorkflow("detail").Equals(WorkflowAction.CancelWorkflow("detail")));
            Assert.True(WorkflowAction.CancelWorkflow("").Equals(WorkflowAction.CancelWorkflow("")));

            Assert.False(WorkflowAction.CancelWorkflow("detail").Equals(WorkflowAction.CancelWorkflow("detail1")));
            Assert.False(WorkflowAction.CancelWorkflow("detail").Equals(WorkflowAction.CancelWorkflow("DETAIL")));
            Assert.False(WorkflowAction.CancelWorkflow("detail").Equals(WorkflowAction.CancelWorkflow(null)));
        }

        [Test]
        public void Should_return_cancel_workflow_decision()
        {
            var action = WorkflowAction.CancelWorkflow("detail");

            var decision = action.GetDecisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new SingleActivityWorkflow("detail");
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion, SingleActivityWorkflow.PositionalName, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.CancelWorkflow("detail")));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public SingleActivityWorkflow(string detail)
            {
                AddActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CancelWorkflow(detail));
            }
        }
    }
}
