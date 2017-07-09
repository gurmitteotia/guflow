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
    public class ScheduleWorkflowItemActionTests
    {
        private readonly Mock<IWorkflow> _workflow = new Mock<IWorkflow>();
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";

        [Test]
        public void Equality_tests()
        {
            Assert.True(WorkflowAction.Schedule(TimerItem.New(Identity.Timer("Somename"),_workflow.Object)).Equals(WorkflowAction.Schedule(TimerItem.New(Identity.Timer("Somename"),_workflow.Object))));
            Assert.False(WorkflowAction.Schedule(TimerItem.New(Identity.Timer("Somename"), _workflow.Object)).Equals(WorkflowAction.Schedule(TimerItem.New(Identity.Timer("Somename1"), _workflow.Object))));
        }
        [Test]
        public void Should_return_the_scheduling_decision_for_workflow_item()
        {
            var workflowItem = TimerItem.New(Identity.Timer("Somename"),_workflow.Object);
            var workflowAction = WorkflowAction.Schedule(workflowItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{workflowItem.GetScheduleDecision()}));
        }

        [Test]
        public void Should_return_timer_decision_when_rescheduled_after_a_timeout()
        {
            var workflowItem = new ActivityItem(Identity.New("name","ver","pos"),_workflow.Object);
            var workflowAction = WorkflowAction.Schedule(workflowItem).After(TimeSpan.FromSeconds(2));

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{new ScheduleTimerDecision(Identity.New("name","ver","pos"),TimeSpan.FromSeconds(2),true)}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnRescheduleAction();
            var completedActivityEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);
            
            var workflowAction = completedActivityEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Schedule(new ActivityItem(Identity.New(_activityName, _activityVersion, _positionalName),null))));
        }

        [Test]
        public void Returns_timer_decision_when_total_number_of_scheduling_is_less_than_allowed_limit()
        {
            var workflow = new WorkflowToScheduleActivityUpToLimit(3);
            var completed1 = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);
            var completed2 = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);
            var historyEvents = new WorkflowHistoryEvents(completed2.Concat(completed1), completed2.Last().EventId, completed2.First().EventId);

            var decisions = historyEvents.InterpretNewEventsFor(workflow);

            Assert.That(decisions, Is.EqualTo(new []{ new CompleteWorkflowDecision("Completed")}));

        
        }

        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private IEnumerable<HistoryEvent> CreateCompletedActivityEventGraph(string activityName, string activityVersion, string positionalName)
        {
            return HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res");
        }
        private class WorkflowToReturnRescheduleAction : Workflow
        {
            public WorkflowToReturnRescheduleAction()
            {
                ScheduleActivity(_activityName, _activityVersion,_positionalName).OnCompletion(Reschedule);
            }
        }

        private class WorkflowToScheduleActivityUpToLimit : Workflow
        {
            public WorkflowToScheduleActivityUpToLimit(uint limit)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName)
                    .OnCompletion(e => Reschedule(e).UpTo(Limit.Count(limit)));

                ScheduleAction(CancelWorkflow("completed")).After(_activityName, _activityVersion, _positionalName);
            }
        }
     
    }
}