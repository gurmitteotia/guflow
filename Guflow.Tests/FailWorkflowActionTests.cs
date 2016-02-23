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
            Assert.True(new FailWorkflowAction("reason","detail").Equals(new FailWorkflowAction("reason","detail")));
            Assert.True(new FailWorkflowAction("", "").Equals(new FailWorkflowAction("", "")));

            Assert.False(new FailWorkflowAction("reason", "detail").Equals(new FailWorkflowAction("reason1", "detail")));
            Assert.False(new FailWorkflowAction("reason", "detail").Equals(new FailWorkflowAction("reason", "detail1")));
            Assert.False(new FailWorkflowAction("reason", "detail").Equals(new FailWorkflowAction(null, "detail")));
        }

        [Test]
        public void Should_return_fail_workflow_decision()
        {
            var action = new FailWorkflowAction("reason", "detail");

            var decision = action.GetDecisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflow = new SingleActivityWorkflow("reason","detail");
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion, SingleActivityWorkflow.PositionalName, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(new FailWorkflowAction("reason", "detail")));
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