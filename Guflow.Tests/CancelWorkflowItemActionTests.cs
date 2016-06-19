using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class CancelWorkflowItemActionTests
    {
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        [Test]
        public void Equality_tests()
        {
            var workflowItem1 = new TimerItem(Identity.Timer("TimerName"),new Mock<IWorkflowItems>().Object);
            var workflowItem2 = new TimerItem(Identity.Timer("TimerName2"),new Mock<IWorkflowItems>().Object);
            Assert.That(WorkflowAction.Cancel(workflowItem1).Equals(WorkflowAction.Cancel(workflowItem1)));
            Assert.False(WorkflowAction.Cancel(workflowItem1).Equals(WorkflowAction.Cancel(workflowItem2)));
        }

        [Test]
        public void Should_return_cancel_timer_decision_for_timer_item()
        {
            var timerItem =new TimerItem(Identity.Timer("TimerName"), new Mock<IWorkflowItems>().Object);
            var workflowAction = WorkflowAction.Cancel(timerItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions,Is.EqualTo(new[]{new CancelTimerDecision(Identity.Timer("TimerName"))}));
        }

        [Test]
        public void Should_return_cancel_activity_decision_for_activity_item()
        {
            var activityItem = new ActivityItem(Identity.New("_timerName","ver","pos"), new Mock<IWorkflowItems>().Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelActivityDecision(Identity.New("_timerName", "ver", "pos")) }));
        }

        [Test]
        public void Can_be_returned_as_cancelled_activity_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelActivityAction();
            var completedActivityEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Cancel(new ActivityItem(Identity.New("ActivityToCancel","1.2"),null))));
        }

        [Test]
        public void Can_be_returned_as_cancelled_timer_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelledTimerAction();
            var completedActivityEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Cancel(new TimerItem(Identity.Timer("SomeTimer"),null))));
        }

        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private class WorkflowToReturnCancelActivityAction : Workflow
        {
            public WorkflowToReturnCancelActivityAction()
            {
                ScheduleActivity("ActivityToCancel", "1.2");
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => CancelActivity("ActivityToCancel", "1.2"));
            }
        }
        private class WorkflowToReturnCancelledTimerAction : Workflow
        {
            public WorkflowToReturnCancelledTimerAction()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => CancelTimer("SomeTimer"));
                ScheduleTimer("SomeTimer");
            }
        }
    }
}