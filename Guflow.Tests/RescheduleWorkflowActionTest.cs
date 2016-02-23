using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class RescheduleWorkflowActionTest
    {
        private readonly Mock<IWorkflowItems> _workflowItems = new Mock<IWorkflowItems>();

        [Test]
        public void Equality_tests()
        {
            Assert.True(new RescheduleWorkflowAction(new TimerItem("Somename",_workflowItems.Object)).Equals(new RescheduleWorkflowAction(new TimerItem("Somename",_workflowItems.Object))));
            Assert.False(new RescheduleWorkflowAction(new TimerItem("Somename", _workflowItems.Object)).Equals(new RescheduleWorkflowAction(new TimerItem("Somename1", _workflowItems.Object))));
        }

        [Test]
        public void Should_return_the_scheduling_decision_for_workflow_item()
        {
            var workflowItem = new TimerItem("Somename",_workflowItems.Object);
            var workflowAction = new RescheduleWorkflowAction(workflowItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{workflowItem.GetDecision()}));
        }

        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflow = new SingleActivityWorkflow();
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion, SingleActivityWorkflow.PositionalName, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(new RescheduleWorkflowAction(workflow.CompletedWorkflowItem)));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public SingleActivityWorkflow()
            {
                CompletedWorkflowItem = AddActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(Reschedule);
            }

            public WorkflowItem CompletedWorkflowItem { get; private set; }
        }
    }
}