using System;
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
            Assert.True(WorkflowAction.Reschedule(new TimerItem("Somename",_workflowItems.Object)).Equals(WorkflowAction.Reschedule(new TimerItem("Somename",_workflowItems.Object))));
            Assert.False(WorkflowAction.Reschedule(new TimerItem("Somename", _workflowItems.Object)).Equals(WorkflowAction.Reschedule(new TimerItem("Somename1", _workflowItems.Object))));
        }
        [Test]
        public void Should_return_the_scheduling_decision_for_workflow_item()
        {
            var workflowItem = new TimerItem("Somename",_workflowItems.Object);
            var workflowAction = WorkflowAction.Reschedule(workflowItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{workflowItem.GetDecision()}));
        }

        [Test]
        public void Should_return_timer_decision_when_rescheduled_after_a_timeout()
        {
            var workflowItem = new ActivityItem("name","ver","pos",_workflowItems.Object);
            var workflowAction = WorkflowAction.Reschedule(workflowItem).After(TimeSpan.FromSeconds(2));

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{new ScheduleTimerDecision(Identity.New("name","ver","pos"),TimeSpan.FromSeconds(2),true)}));
        }

        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflow = new WorkflowToReturnRescheduleAction();
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(WorkflowToReturnRescheduleAction.ActivityName, WorkflowToReturnRescheduleAction.ActivityVersion, WorkflowToReturnRescheduleAction.PositionalName, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Reschedule(workflow.CompletedWorkflowItem)));
        }
        private class WorkflowToReturnRescheduleAction : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public WorkflowToReturnRescheduleAction()
            {
                CompletedWorkflowItem = AddActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(Reschedule);
            }
            public WorkflowItem CompletedWorkflowItem { get; private set; }
        }
    }
}