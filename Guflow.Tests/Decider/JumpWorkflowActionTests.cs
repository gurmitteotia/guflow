using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class JumpWorkflowActionTests
    {
        private readonly Mock<IWorkflow> _workflow = new Mock<IWorkflow>();
        private const string ActivityName = "activity";
        private const string ActivityVersion = "2.0";
        private const string PositionalName = "pname";
        private const string SiblingActivityName = "BookHotelActivity";
        private const string Version = "1.0";
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

            Assert.That(decisions, Is.EquivalentTo(workflowItem.GetScheduleDecisions()));
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
            var completedActivityEvent = CreateCompletedActivityEvent(ActivityName, ActivityVersion, PositionalName);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.JumpTo(new ActivityItem(Identity.New(ActivityName, ActivityVersion, PositionalName), null))));
        }

        [Test]
        public void Can_be_returned_as_workflow_action_when_scheduling_the_timer()
        {
            var workflow = new WorkflowToReturnScheduleTimerAction();
            var completedActivityEvent = CreateCompletedActivityEvent(ActivityName, ActivityVersion, PositionalName);

            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.JumpTo(TimerItem.New(Identity.Timer("SomeTimer"), null))));
        }

        [Test]
        public void Jumping_out_to_different_parent_branch_is_not_allowed()
        {
            var siblingActivity = CompletedActivityGraph(SiblingActivityName);
            var workflow = new WorkflowToJumpToDifferentBranch();
            var historyEvents = new WorkflowHistoryEvents(siblingActivity, siblingActivity.Last().EventId, siblingActivity.First().EventId);

            Assert.Throws<OutOfBranchJumpException>(()=> workflow.NewExecutionFor(historyEvents).Execute());
        }

        private class WorkflowToReturnScheduleActivityAction : Workflow
        {
            public WorkflowToReturnScheduleActivityAction()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => Jump.ToActivity(ActivityName, ActivityVersion, PositionalName));
            }
        }
        private class WorkflowToReturnScheduleTimerAction : Workflow
        {
            public WorkflowToReturnScheduleTimerAction()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => Jump.ToTimer("SomeTimer"));
                ScheduleTimer("SomeTimer").AfterActivity(ActivityName, ActivityVersion, PositionalName);
            }
        }
        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private static IEnumerable<HistoryEvent> CompletedActivityGraph(string activityName)
        {
            return HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, Version), "id", "result");
        }
        [WorkflowDescription("1.0")]
        private class WorkflowToJumpToDifferentBranch : Workflow
        {
            public WorkflowToJumpToDifferentBranch()
            {
                ScheduleActivity(ActivityName, Version);

                ScheduleActivity(SiblingActivityName, Version)
                    .OnCompletion(e => Jump.ToActivity(ActivityName, Version));
            }
        }
    }
}