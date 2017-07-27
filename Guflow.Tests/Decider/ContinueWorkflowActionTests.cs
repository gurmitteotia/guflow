using System;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
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
            var workflowItem1 = new ActivityItem(Identity.New( _activityName, _activityVersion, _positionalName), new Mock<IWorkflow>().Object);
            var workflowItem2 = new ActivityItem(Identity.New("DifferentName", _activityVersion, _positionalName), new Mock<IWorkflow>().Object);

            Assert.True(WorkflowAction.ContinueWorkflow(workflowItem1).Equals(WorkflowAction.ContinueWorkflow(workflowItem1)));
            Assert.False(WorkflowAction.ContinueWorkflow(workflowItem1).Equals(WorkflowAction.ContinueWorkflow(workflowItem2)));
        }

        [Test]
        public void Returns_the_scheduling_decision_for_all_child_activities()
        {
            var workflow = new WorkflowWithMultipleChilds();
            var workflowHistoryEvents = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.NewExecutionFor(workflowHistoryEvents).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")), new ScheduleActivityDecision(Identity.New("Sync", "2.1")) }));
        }

        [Test]
        public void Returns_propose_to_complete_complete_decision_when_no_schedulable_child_item_found()
        {
            var workflow = new SingleActivityWorkflow();
            var workflowHistoryEvents = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflowHistoryEvents.InterpretNewEventsFor(workflow);

            Assert.That(decisions,Is.EqualTo(new []{new CompleteWorkflowDecision("Workflow is completed.",true)}));
        }
        [Test]
        public void Schedule_the_child_when_one_of_its_parents_branch_is_completed_and_other_parent_branch_is_not_active_at_all()
        {
            var workflow = new WorkflowWithMultipleParents();
            var workflowHistoryEvents = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.NewExecutionFor(workflowHistoryEvents).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0"))}));

        }
        [Test]
        public void Does_not_schedule_the_child_when_one_of_its_parent_activity_ignores_the_action_by_keeping_the_branch_active()
        {
            var workflow = new WorkflowWithAParentIgnoringCompleteEvent(keepBranchActive:true);
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res")
                                  .Concat(HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2"));

            var decisions = workflow.NewExecutionFor(new WorkflowHistoryHistoryEvents(allHistoryEvents)).Execute();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Does_not_schedule_the_child_when_one_of_its_parent_activity_is_active()
        {
            var workflow = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res")
                                 .Concat(HistoryEventFactory.CreateActivityScheduledEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion)));

            var decisions = workflow.NewExecutionFor(new WorkflowHistoryHistoryEvents(allHistoryEvents)).Execute();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_all_of_its_parents_are_completed()
        {
            var workflow = new WorkflowWithMultipleParents();
            var activityGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res");
            var siblingGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2");


            var decisions = workflow.NewExecutionFor(new WorkflowHistoryHistoryEvents(siblingGraph.Concat(activityGraph))).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }
        [Test]
        public void Return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_other_one_is_failed_but_configured_to_continue()
        {
            var workflow = new WorkflowWithAParentContinueOnFailure();
            var allHistoryEvents = CreateParentActivityCompletedAndFailedEventsGraph();
           
            var decisions = workflow.NewExecutionFor(allHistoryEvents).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_other_one_is_timedout_but_configured_to_continue()
        {
            var workflow = new WorkflowWithAParentContinueOnTimedout();
            var allHistoryEvents = CreateParentActivityCompletedAndTimedoutEventsGraph();

            var decisions = workflow.NewExecutionFor(allHistoryEvents).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_cancelled_but_configured_to_continue()
        {
            var workflow = new WorkflowWithAParentContinueOnCancelled();
            var allHistoryEvents = CreateParentActivityCompletedAndCancelledEventsGraph();

            var decisions = workflow.NewExecutionFor(allHistoryEvents).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_timer_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent", childTimer = "child";
            var childTimeout = TimeSpan.FromSeconds(2);
            var workflow = new WorkflowWithParentChildTimers(parentTimer, childTimer,childTimeout);
            var timerFiredEventGraph = CreateTimerFiredEventGraph(parentTimer);

            var decisions = workflow.NewExecutionFor(timerFiredEventGraph).Execute();


            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.Timer(childTimer), childTimeout) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_activity_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent";
            var workflow = new WorkflowWithChildActivity(parentTimer);
            var timerFiredEventGraph = CreateTimerFiredEventGraph(parentTimer);

            var decisions = workflow.NewExecutionFor(timerFiredEventGraph).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New(_activityName, _activityVersion)) }));
        }

        [Test]
        public void Can_return_the_scheduling_decision_for_child_timer_when_parent_activity_is_completed()
        {
            const string timerName = "timer";
            var workflow = new WorkflowWithParentActivityAndChildTimers(timerName);
            var completedActivityEventGraph = CreateCompletedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.NewExecutionFor(completedActivityEventGraph).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.Timer(timerName),new TimeSpan())}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new WorkflowWithCustomContinue();
            var activityFailedEvent = CreateFailedActivityEventGraph(_activityName, _activityVersion, _positionalName);

            var decisions = activityFailedEvent.InterpretNewEventsFor(workflow);

            Assert.That(decisions,Is.EquivalentTo(WorkflowAction.ContinueWorkflow(new ActivityItem(Identity.New(_activityName,_activityVersion,_positionalName),workflow)).GetDecisions()));
        }


        [Test]
        public void Can_return_scheduling_decision_for_workflow_action_when_all_of_its_parents_are_completed()
        {
            var workflow = new WorkflowForSchedulableWorkflowActionWithMultipleParents("result");
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res");

            var siblingActivityCompletedGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2");

            var decisions = workflow.NewExecutionFor(new WorkflowHistoryHistoryEvents(siblingActivityCompletedGraph.Concat(activityCompletedEventGraph))).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[] { new CompleteWorkflowDecision("result")}));
        }

        private IWorkflowHistoryEvents CreateFailedActivityEventGraph(string activityName, string activityVersion, string positionalName)
        {
            return new WorkflowHistoryHistoryEvents(HistoryEventFactory.CreateActivityFailedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res", "detail"));
        }
        private IWorkflowHistoryEvents CreateCompletedActivityEventGraph(string activityName, string activityVersion, string positionalName)
        {
            return new WorkflowHistoryHistoryEvents(HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res"));
        }
        private IWorkflowHistoryEvents CreateActivityCompletedAndActivityStartedEventGraph()
        {
            return new WorkflowHistoryHistoryEvents(HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res")
                                            .Concat(HistoryEventFactory.CreateActivityStartedEventGraph(Identity.New(_siblingActivityName,_siblingActivityVersion),"id")));
        }

        private IWorkflowHistoryEvents CreateTimerFiredEventGraph(string timerName)
        {
            return new WorkflowHistoryHistoryEvents(HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer(timerName), TimeSpan.FromSeconds(2)));
        }

        private IWorkflowHistoryEvents CreateParentActivityCompletedAndFailedEventsGraph()
        {
            var activityFailedEventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2", "detail");
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res");
            return new WorkflowHistoryHistoryEvents(activityCompletedEventGraph.Concat(activityFailedEventGraph), activityCompletedEventGraph.Last().EventId, activityCompletedEventGraph.First().EventId);
        }

        private IWorkflowHistoryEvents CreateParentActivityCompletedAndTimedoutEventsGraph()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res");
            var activityTimedoutEventGraph = HistoryEventFactory.CreateActivityTimedoutEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2", "detail");
            return new WorkflowHistoryHistoryEvents(activityCompletedEventGraph.Concat(activityTimedoutEventGraph), activityCompletedEventGraph.Last().EventId, activityCompletedEventGraph.First().EventId);
        }
        private IWorkflowHistoryEvents CreateParentActivityCompletedAndCancelledEventsGraph()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), "id", "res");
            var activityCancelledEventGraph = HistoryEventFactory.CreateActivityCancelledEventGraph(Identity.New(_siblingActivityName, _siblingActivityVersion), "id2", "re2");
            return new WorkflowHistoryHistoryEvents(activityCompletedEventGraph.Concat(activityCancelledEventGraph), activityCompletedEventGraph.Last().EventId, activityCompletedEventGraph.First().EventId);
        }
        private class WorkflowWithMultipleChilds : Workflow
        {
            public WorkflowWithMultipleChilds()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);

                ScheduleActivity("Transcode", "2.0").After(_activityName, _activityVersion, _positionalName);
                ScheduleActivity("Sync", "2.1").After(_activityName, _activityVersion, _positionalName);
            }
        }
        private class WorkflowWithMultipleParents : Workflow
        {
            public WorkflowWithMultipleParents()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion);
                ScheduleActivity("Transcode", "2.0").After(_activityName, _activityVersion, _positionalName).After(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class WorkflowForSchedulableWorkflowActionWithMultipleParents : Workflow
        {
            public WorkflowForSchedulableWorkflowActionWithMultipleParents(string workflowActionResult)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion);
                ScheduleAction(CompleteWorkflow(workflowActionResult)).After(_activityName, _activityVersion, _positionalName).After(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class WorkflowWithAParentContinueOnFailure : Workflow
        {
            public WorkflowWithAParentContinueOnFailure()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion).OnFailure(Continue);
                ScheduleActivity("Transcode", "2.0").After(_activityName, _activityVersion, _positionalName).After(_siblingActivityName, _siblingActivityVersion);
            }
        }
        private class WorkflowWithAParentContinueOnTimedout : Workflow
        {
            public WorkflowWithAParentContinueOnTimedout()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion).OnTimedout(Continue);
                ScheduleActivity("Transcode", "2.0").After(_activityName, _activityVersion, _positionalName).After(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class WorkflowWithAParentContinueOnCancelled : Workflow
        {
            public WorkflowWithAParentContinueOnCancelled()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion).OnCancelled(Continue);
                ScheduleActivity("Transcode", "2.0").After(_activityName, _activityVersion, _positionalName).After(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class WorkflowWithAParentIgnoringCompleteEvent : Workflow
        {
            public WorkflowWithAParentIgnoringCompleteEvent(bool keepBranchActive)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion).OnCompletion(e => Ignore(keepBranchActive));
                ScheduleActivity("Transcode", "2.0").After(_activityName, _activityVersion, _positionalName).After(_siblingActivityName, _siblingActivityVersion);
            }
        }
        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
            }
        }

        private class WorkflowWithMutlipleStartup : Workflow
        {
            public WorkflowWithMutlipleStartup()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion);
            }
        }
        private class WorkflowWithCustomContinue : Workflow
        {
            public WorkflowWithCustomContinue()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnFailure(Continue);
            }
        }

        private class WorkflowWithParentChildTimers : Workflow
        {
            public WorkflowWithParentChildTimers(string timerName, string childTimer,TimeSpan childTimeout)
            {
                ScheduleTimer(timerName);
                ScheduleTimer(childTimer).After(timerName).FireAfter(childTimeout);
            }
        }
        private class WorkflowWithChildActivity : Workflow
        {
            public WorkflowWithChildActivity(string timerName)
            {
                ScheduleTimer(timerName);
                ScheduleActivity(_activityName, _activityVersion).After(timerName);
            }
        }
        private class WorkflowWithParentActivityAndChildTimers : Workflow
        {
            public WorkflowWithParentActivityAndChildTimers(string timerName)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleTimer(timerName).After(_activityName, _activityVersion,_positionalName);
            }
        }
    }
}