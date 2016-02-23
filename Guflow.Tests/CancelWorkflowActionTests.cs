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
            Assert.True(new CancelWorkflowAction("detail").Equals(new CancelWorkflowAction("detail")));
            Assert.True(new CancelWorkflowAction("").Equals(new CancelWorkflowAction("")));

            Assert.False(new CancelWorkflowAction("detail").Equals(new CancelWorkflowAction("detail1")));
            Assert.False(new CancelWorkflowAction("detail").Equals(new CancelWorkflowAction("DETAIL")));
            Assert.False(new CancelWorkflowAction("detail").Equals(new CancelWorkflowAction(null)));
        }

        [Test]
        public void Should_return_cancel_workflow_decision()
        {
            var action = new CancelWorkflowAction("detail");

            var decision = action.GetDecisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflow = new SingleActivityWorkflow("detail");
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion, SingleActivityWorkflow.PositionalName, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(new CancelWorkflowAction("detail")));
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
