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

            Assert.True(WorkflowAction.ContinueWorkflow(workflowItem1).Equals(WorkflowAction.ContinueWorkflow(workflowItem1)));
            Assert.False(WorkflowAction.ContinueWorkflow(workflowItem1).Equals(WorkflowAction.ContinueWorkflow(workflowItem2)));
        }

        [Test]
        public void Should_return_the_scheduling_decision_for_all_child_activities()
        {
            var workflow = new WorkflowWithMultipleChilds();
            var workflowHistoryEvents = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.ExecuteFor(workflowHistoryEvents);

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")), new ScheduleActivityDecision(Identity.New("Sync", "2.1")) }));
        }

        [Test]
        public void Should_return_empty_decision_when_no_schedulable_child_item_found()
        {
            var workflow = new SingleActivityWorkflow();
            var workflowHistoryEvents = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.ExecuteFor(workflowHistoryEvents);


            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Should_not_schedule_the_child_when_one_of_its_parent_is_not_completed()
        {
            var workflow = new WorkflowWithMultipleParents();
            var workflowHistoryEvents = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.ExecuteFor(workflowHistoryEvents);

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_all_of_its_parents_are_completed()
        {
            var workflow = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2"));


            var decisions = workflow.ExecuteFor(new WorkflowHistoryEvents(allHistoryEvents));

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_failed()
        {
            var workflow = new WorkflowWithMultipleParents();
            var allHistoryEvents = CreateParentActivityCompletedAndFailedEventsGraph();
           
            var decisions = workflow.ExecuteFor(allHistoryEvents);

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_timedout()
        {
            var workflow = new WorkflowWithMultipleParents();
            var allHistoryEvents = CreateParentActivityCompletedAndTimedoutEventsGraph();

            var decisions = workflow.ExecuteFor(allHistoryEvents);

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_cancelled()
        {
            var workflow = new WorkflowWithMultipleParents();
            var allHistoryEvents = CreateParentActivityCompletedAndCancelledEventsGraph();

            var decisions = workflow.ExecuteFor(allHistoryEvents);

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_timer_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent", childTimer = "child";
            var childTimeout = TimeSpan.FromSeconds(2);
            var workflow = new WorkflowWithParentChildTimers(parentTimer, childTimer,childTimeout);
            var timerFiredEventGraph = CreateTimerFiredEventGraph(parentTimer);

            var decisions = workflow.ExecuteFor(timerFiredEventGraph);


            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.Timer(childTimer), childTimeout) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_activity_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent";
            var workflow = new WorkflowWithChildActivity(parentTimer);
            var timerFiredEventGraph = CreateTimerFiredEventGraph(parentTimer);

            var decisions = workflow.ExecuteFor(timerFiredEventGraph);

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New(_activityName, _activityVersion)) }));
        }

        [Test]
        public void Should_return_the_scheduling_decision_for_child_timer_when_parent_activity_is_completed()
        {
            const string timerName = "timer";
            var workflow = new WorkflowWithParentActivityAndChildTimers(timerName);
            var completedActivityEventGraph = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.ExecuteFor(completedActivityEventGraph);

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.Timer(timerName),new TimeSpan())}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new WorkflowWithCustomContinue();
            var activityFailedEvent = CreateFailedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.ExecuteFor(activityFailedEvent);

            Assert.That(decisions,Is.EquivalentTo(WorkflowAction.ContinueWorkflow(workflow.FailedItem).GetDecisions()));
        }


        private IWorkflowHistoryEvents CreateFailedActivityEventGraph(string activityName, string activityVersion, string positionalName)
        {
            return new WorkflowHistoryEvents(HistoryEventFactory.CreateActivityFailedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res", "detail"));
        }
        private IWorkflowHistoryEvents CreateCompletedActivityEventGraph(string activityName, string activityVersion, string positionalName)
        {
            return new WorkflowHistoryEvents(HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res"));
        }

        private IWorkflowHistoryEvents CreateTimerFiredEventGraph(string timerName)
        {
            return new WorkflowHistoryEvents(HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer(timerName), TimeSpan.FromSeconds(2)));
        }

        private IWorkflowHistoryEvents CreateParentActivityCompletedAndFailedEventsGraph()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res");
            var activityFailedEventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2", "detail");
            return new WorkflowHistoryEvents(activityCompletedEventGraph.Concat(activityFailedEventGraph), activityCompletedEventGraph.First().EventId, activityCompletedEventGraph.Last().EventId);
        }

        private IWorkflowHistoryEvents CreateParentActivityCompletedAndTimedoutEventsGraph()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res");
            var activityTimedoutEventGraph = HistoryEventFactory.CreateActivityTimedoutEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2", "detail");
            return new WorkflowHistoryEvents(activityCompletedEventGraph.Concat(activityTimedoutEventGraph), activityCompletedEventGraph.First().EventId, activityCompletedEventGraph.Last().EventId);
        }
        private IWorkflowHistoryEvents CreateParentActivityCompletedAndCancelledEventsGraph()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res");
            var activityCancelledEventGraph = HistoryEventFactory.CreateActivityCancelledEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2");
            return new WorkflowHistoryEvents(activityCompletedEventGraph.Concat(activityCancelledEventGraph), activityCompletedEventGraph.First().EventId, activityCompletedEventGraph.Last().EventId);
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