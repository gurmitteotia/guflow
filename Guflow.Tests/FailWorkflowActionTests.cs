using NUnit.Framework;
using System.Linq;

namespace Guflow.Tests
{
    [TestFixture]
    public class FailWorkflowActionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(WorkflowAction.FailWorkflow("reason", "detail").Equals(WorkflowAction.FailWorkflow("reason", "detail")));
            Assert.True(WorkflowAction.FailWorkflow("", "").Equals(WorkflowAction.FailWorkflow("", "")));

            Assert.False(WorkflowAction.FailWorkflow("reason", "detail").Equals(WorkflowAction.FailWorkflow("reason1", "detail")));
            Assert.False(WorkflowAction.FailWorkflow("reason", "detail").Equals(WorkflowAction.FailWorkflow("reason", "detail1")));
            Assert.False(WorkflowAction.FailWorkflow("reason", "detail").Equals(WorkflowAction.FailWorkflow(null, "detail")));
        }

        [Test]
        public void Should_return_fail_workflow_decision()
        {
            var action = WorkflowAction.FailWorkflow("reason", "detail");

            var decision = action.GetDecisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new SingleActivityWorkflow("reason","detail");
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion, SingleActivityWorkflow.PositionalName), "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.FailWorkflow("reason", "detail")));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public SingleActivityWorkflow(string reason, string detail)
            {
                AddActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => FailWorkflow(reason, detail));
            }
        }
    }
}