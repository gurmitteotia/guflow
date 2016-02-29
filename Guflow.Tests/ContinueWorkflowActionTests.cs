using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ContinueWorkflowActionTests
    {
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _siblingActivityName = "Sync";
        private const string _siblingActivityVersion = "2.0";

        [Test]
        public void Equality_tests()
        {
            var workflowItem1 = new ActivityItem(_activityName, _activityVersion, _positionalName, new Mock<IWorkflowItems>().Object);
            var workflowItem2 = new ActivityItem("DifferentName", _activityVersion, _positionalName, new Mock<IWorkflowItems>().Object);

            Assert.True(WorkflowAction.ContinueWorkflow(workflowItem1, new Mock<IWorkflowContext>().Object).Equals(WorkflowAction.ContinueWorkflow(workflowItem1, new Mock<IWorkflowContext>().Object)));
            Assert.False(WorkflowAction.ContinueWorkflow(workflowItem1, new Mock<IWorkflowContext>().Object).Equals(WorkflowAction.ContinueWorkflow(workflowItem2, new Mock<IWorkflowContext>().Object)));
        }

        [Test]
        public void Should_return_the_scheduling_decision_for_all_child_activities()
        {
            var workflow = new WorkflowWithMultipleChilds();
            var activityFailedEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = activityFailedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0"), new ScheduleActivityDecision("Sync", "2.1") }));
        }

        [Test]
        public void Should_return_empty_decision_when_no_schedulable_child_item_found()
        {
            var workflow = new SingleActivityWorkflow();
            var activityFailedEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = activityFailedEvent.Interpret(workflow).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Should_not_schedule_the_child_when_one_of_its_parent_is_not_completed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var activityFailedEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);
           
            var decisions = activityFailedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_all_of_its_parents_are_completed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityCompletedEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_failed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityFailedEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_timedout()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityTimedoutEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_cancelled()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityCancelledEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_timer_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent", childTimer = "child";
            var childTimeout = TimeSpan.FromSeconds(2);
            var workflow = new WorkflowWithParentChildTimers(parentTimer, childTimer,childTimeout);
            var timerFiredEvent = CreateTimerFiredEvent(parentTimer);

            var decisions = timerFiredEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.Timer(childTimer), childTimeout) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_activity_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent";
            var workflow = new WorkflowWithChildActivity(parentTimer);
            var timerFiredEvent = CreateTimerFiredEvent(parentTimer);

            var decisions = timerFiredEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(_activityName, _activityVersion) }));
        }

        [Test]
        public void Should_return_the_scheduling_decision_for_child_timer_when_parent_activity_is_completed()
        {
            const string timerName = "timer";
            var workflow = new WorkflowWithParentActivityAndChildTimers(timerName);
            var activityFailedEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = activityFailedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.Timer(timerName),new TimeSpan())}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new WorkflowWithCustomContinue();
            var activityFailedEvent = CreateFailedActivityEvent(_activityName, _activityVersion, _positionalName);

            var workflowAction = activityFailedEvent.Interpret(workflow);
            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.ContinueWorkflow(workflow.FailedItem,new Mock<IWorkflowContext>().Object)));
        }


        private ActivityFailedEvent CreateFailedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityFailedEventGraph(activityName, activityVersion, positionalName, "id", "res","detail");
            return new ActivityFailedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(activityName, activityVersion, positionalName, "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private TimerFiredEvent CreateTimerFiredEvent(string timerName)
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer(timerName), TimeSpan.FromSeconds(2));
            return new TimerFiredEvent(timerFiredEventGraph.First(),timerFiredEventGraph);
        }
        private class WorkflowWithMultipleChilds : Workflow
        {
            public WorkflowWithMultipleChilds()
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);

                AddActivity("Transcode", "2.0").DependsOn(_activityName, _activityVersion, _positionalName);
                AddActivity("Sync", "2.1").DependsOn(_activityName, _activityVersion, _positionalName);
            }
        }
        private class WorkflowWithMultipleParents : Workflow
        {
            public WorkflowWithMultipleParents()
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                AddActivity(_siblingActivityName, _siblingActivityVersion);
                AddActivity("Transcode", "2.0").DependsOn(_activityName, _activityVersion, _positionalName).DependsOn(_siblingActivityName, _siblingActivityVersion);
            }
        }
        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
            }
        }
        private class WorkflowWithCustomContinue : Workflow
        {
            public WorkflowWithCustomContinue()
            {
                FailedItem = AddActivity(_activityName, _activityVersion, _positionalName).OnFailure(Continue);
            }
            public WorkflowItem FailedItem { get; private set; }
        }

        private class WorkflowWithParentChildTimers : Workflow
        {
            public WorkflowWithParentChildTimers(string timerName, string childTimer,TimeSpan childTimeout)
            {
                AddTimer(timerName);
                AddTimer(childTimer).DependsOn(timerName).FireAfter(childTimeout);
            }
        }
        private class WorkflowWithChildActivity : Workflow
        {
            public WorkflowWithChildActivity(string timerName)
            {
                AddTimer(timerName);
                AddActivity(_activityName, _activityVersion).DependsOn(timerName);
            }
        }
        private class WorkflowWithParentActivityAndChildTimers : Workflow
        {
            public WorkflowWithParentActivityAndChildTimers(string timerName)
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                AddTimer(timerName).DependsOn(_activityName, _activityVersion,_positionalName);
            }
        }
    }
}