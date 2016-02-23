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
            Assert.That(new CompleteWorkflowAction("result").Equals(new CompleteWorkflowAction("result")));
            Assert.That(new CompleteWorkflowAction("").Equals(new CompleteWorkflowAction("")));
            Assert.That(new CompleteWorkflowAction(null).Equals(new CompleteWorkflowAction(null)));


            Assert.False(new CompleteWorkflowAction("result").Equals(new CompleteWorkflowAction("result1")));
            Assert.False(new CompleteWorkflowAction("result").Equals(new CompleteWorkflowAction("")));
            Assert.False(new CompleteWorkflowAction("result").Equals(new CompleteWorkflowAction(null)));
        }

        [Test]
        public void Should_result_complete_workflow_decision()
        {
            var workflowAction = new CompleteWorkflowAction("result");

            var decision = workflowAction.GetDecisions();

            Assert.That(decision,Is.EquivalentTo(new []{new CompleteWorkflowDecision("result")}));
        }

        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflow = new SingleActivityWorkflow("result");
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion, SingleActivityWorkflow.PositionalName, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(new CompleteWorkflowAction("result")));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public SingleActivityWorkflow(string result)
            {
                AddActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CompleteWorkflow(result));
            }
        }
    }
}
