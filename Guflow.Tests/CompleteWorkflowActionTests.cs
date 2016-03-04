using System.Linq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class CompleteWorkflowActionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.That(WorkflowAction.CompleteWorkflow("result").Equals(WorkflowAction.CompleteWorkflow("result")));
            Assert.That(WorkflowAction.CompleteWorkflow("").Equals(WorkflowAction.CompleteWorkflow("")));
            Assert.That(WorkflowAction.CompleteWorkflow(null).Equals(WorkflowAction.CompleteWorkflow(null)));


            Assert.False(WorkflowAction.CompleteWorkflow("result").Equals(WorkflowAction.CompleteWorkflow("result1")));
            Assert.False(WorkflowAction.CompleteWorkflow("result").Equals(WorkflowAction.CompleteWorkflow("")));
            Assert.False(WorkflowAction.CompleteWorkflow("result").Equals(WorkflowAction.CompleteWorkflow(null)));
        }

        [Test]
        public void Should_return_complete_workflow_decision()
        {
            var workflowAction = WorkflowAction.CompleteWorkflow("result");

            var decision = workflowAction.GetDecisions();

            Assert.That(decision,Is.EquivalentTo(new []{new CompleteWorkflowDecision("result")}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new WorkflowReturningCompleteWorkflowAction("result");
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(WorkflowReturningCompleteWorkflowAction.ActivityName, WorkflowReturningCompleteWorkflowAction.ActivityVersion, WorkflowReturningCompleteWorkflowAction.PositionalName), "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.CompleteWorkflow("result")));
        }

        private class WorkflowReturningCompleteWorkflowAction : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public WorkflowReturningCompleteWorkflowAction(string result)
            {
                AddActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CompleteWorkflow(result));
            }
        }
    }
}
