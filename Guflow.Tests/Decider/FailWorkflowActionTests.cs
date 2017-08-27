using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class FailWorkflowActionTests
    {
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

            var decisions = completedActivityEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("reason", "detail")}));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public SingleActivityWorkflow(string reason, string detail)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => FailWorkflow(reason, detail));
            }
        }
    }
}