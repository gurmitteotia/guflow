using System;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class JumpWorkflowActionTests
    {
        private readonly Mock<IWorkflow> _workflow = new Mock<IWorkflow>();
        private const string _activityName = "activity";
        private const string _activityVersion = "2.0";
        private const string _positionalName = "pname";
        [Test]
        public void Equality_tests()
        {
            Assert.True(WorkflowAction.JumpTo(TimerItem.New(Identity.Timer("Somename"), _workflow.Object)).Equals(WorkflowAction.JumpTo(TimerItem.New(Identity.Timer("Somename"), _workflow.Object))));
            Assert.False(WorkflowAction.JumpTo(TimerItem.New(Identity.Timer("Somename"), _workflow.Object)).Equals(WorkflowAction.JumpTo(TimerItem.New(Identity.Timer("Somename1"), _workflow.Object))));
        }

        [Test]
        public void Returns_the_scheduling_decision_for_workflow_item()
        {
            var workflowItem = TimerItem.New(Identity.Timer("Somename"), _workflow.Object);
            var workflowAction = WorkflowAction.JumpTo(workflowItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { workflowItem.GetScheduleDecision() }));
        }

        [Test]
        public void Returns_timer_decision_when_rescheduled_after_a_timeout()
        {
            var workflowItem = new ActivityItem(Identity.New("name", "ver", "pos"), _workflow.Object);
            var workflowAction = WorkflowAction.JumpTo(workflowItem).After(TimeSpan.FromSeconds(2));

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.New("name", "ver", "pos"), TimeSpan.FromSeconds(2), true) }));
        }

        [Test]
        public void Can_be_returned_as_workflow_action_when_scheduling_the_activity()
        {
            var workflow = new WorkflowToReturnScheduleActivityAction();
            var completedActivityEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.JumpTo(new ActivityItem(Identity.New(_activityName, _activityVersion, _positionalName), null))));
        }

        [Test]
        public void Can_be_returned_as_workflow_action_when_scheduling_the_timer()
        {
            var workflow = new WorkflowToReturnScheduleTimerAction();
            var completedActivityEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.JumpTo(TimerItem.New(Identity.Timer("SomeTimer"), null))));
        }

        private class WorkflowToReturnScheduleActivityAction : Workflow
        {
            public WorkflowToReturnScheduleActivityAction()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => Jump.ToActivity(_activityName, _activityVersion, _positionalName));
            }
        }
        private class WorkflowToReturnScheduleTimerAction : Workflow
        {
            public WorkflowToReturnScheduleTimerAction()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => Jump.ToTimer("SomeTimer"));
                ScheduleTimer("SomeTimer");
            }
        }
        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
    }
}