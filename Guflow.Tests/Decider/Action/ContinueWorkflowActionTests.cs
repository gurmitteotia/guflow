// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Worker;
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
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;

        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
        }

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
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            var decisions = new WorkflowWithMultipleChilds().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")), new ScheduleActivityDecision(Identity.New("Sync", "2.1")) }));
        }

        [Test]
        public void Returns_complete_workflow_decision_when_no_schedulable_child_item_found()
        {
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));

            var decisions = new SingleActivityWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions ,Is.EqualTo(new []{new CompleteWorkflowDecision("Workflow is completed.")}));
        }
        [Test]
        public void Schedule_the_child_when_one_of_its_parents_branch_is_completed_and_other_parent_branch_is_not_active_at_all()
        {
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));

            var decisions = new WorkflowWithMultipleParents().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0"))}));

        }
        [Test]
        public void Does_not_schedule_the_child_when_one_of_its_parent_activity_ignores_the_action_by_keeping_the_branch_active()
        {
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_siblingActivityName, _siblingActivityVersion));
            
            var workflow = new WorkflowWithAParentIgnoringCompleteEvent();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Does_not_schedule_the_child_when_one_of_its_parent_activity_is_active()
        {
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .ActivityScheduledGraph(Identity.New(_siblingActivityName, _siblingActivityVersion))
                .ToArray());

            var workflow = new WorkflowWithMultipleParents();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_all_of_its_parents_are_completed()
        {
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_siblingActivityName, _siblingActivityVersion));
          
            var workflow = new WorkflowWithMultipleParents();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }
        [Test]
        public void Return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_other_one_is_failed_but_configured_to_continue()
        {
            _eventsBuilder.AddProcessedEvents(FailedActivityEventGraph(_siblingActivityName, _siblingActivityVersion));
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            var workflow = new WorkflowWithAParentContinueOnFailure();
           
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_other_one_is_timedout_but_configured_to_continue()
        {
            _eventsBuilder.AddProcessedEvents(TimedoutActivityEventGraph(_siblingActivityName, _siblingActivityVersion));
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            var workflow = new WorkflowWithAParentContinueOnTimedout();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_cancelled_but_configured_to_continue()
        {
            _eventsBuilder.AddProcessedEvents(CancelledActivityEventGraph(_siblingActivityName,
                _siblingActivityVersion));
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            var workflow = new WorkflowWithAParentContinueOnCancelled();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0")) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_timer_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent", childTimer = "child";
            var childTimeout = TimeSpan.FromSeconds(2);
            var workflow = new WorkflowWithParentChildTimers(parentTimer, childTimer,childTimeout);
            _eventsBuilder.AddNewEvents(TimerFiredEventGraph(parentTimer));
            var decisions = workflow.Decisions(_eventsBuilder.Result());


            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.Timer(childTimer).ScheduleId(), childTimeout) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_activity_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent";
            var workflow = new WorkflowWithChildActivity(parentTimer);
            _eventsBuilder.AddNewEvents(TimerFiredEventGraph(parentTimer));

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New(_activityName, _activityVersion)) }));
        }

        [Test]
        public void Can_return_the_scheduling_decision_for_child_timer_when_parent_activity_is_completed()
        {
            const string timerName = "timer";
            var workflow = new WorkflowWithParentActivityAndChildTimers(timerName);
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.Timer(timerName).ScheduleId(),new TimeSpan())}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new WorkflowWithCustomContinue();
            _eventsBuilder.AddNewEvents(FailedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new CompleteWorkflowDecision("Workflow is completed.") }));
        }


        [Test]
        public void Can_return_scheduling_decision_for_workflow_action_when_all_of_its_parents_are_completed()
        {
            var workflow = new WorkflowForSchedulableWorkflowActionWithMultipleParents("result");
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion, _positionalName));
            _eventsBuilder.AddNewEvents(CompletedActivityEventGraph(_siblingActivityName, _siblingActivityVersion));

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[] { new CompleteWorkflowDecision("result")}));
        }

        private HistoryEvent [] FailedActivityEventGraph(string activityName, string activityVersion, string positionalName ="")
        {
            return _eventGraphBuilder.ActivityFailedGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res", "detail").ToArray();
        }
        private HistoryEvent[] CompletedActivityEventGraph(string activityName, string activityVersion, string positionalName ="")
        {
            return _eventGraphBuilder.ActivityCompletedGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res").ToArray();
        }

        private HistoryEvent[] CancelledActivityEventGraph(string activityName, string activityVersion, string positionalName = "")
        {
            return _eventGraphBuilder.ActivityCancelledGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res").ToArray();
        }

        private HistoryEvent[] TimedoutActivityEventGraph(string activityName, string activityVersion, string positionalName = "")
        {
            return _eventGraphBuilder.ActivityTimedoutGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res", "det").ToArray();
        }

        private HistoryEvent[] TimerFiredEventGraph(string timerName)
        {
            return _eventGraphBuilder.TimerFiredGraph(Identity.Timer(timerName).ScheduleId(), TimeSpan.FromSeconds(2)).ToArray();
        }
    
        private class WorkflowWithMultipleChilds : Workflow
        {
            public WorkflowWithMultipleChilds()
            {
                ScheduleActivity<TestActivity>(_positionalName).OnCompletion(Continue);

                ScheduleActivity("Transcode", "2.0").AfterActivity<TestActivity>(_positionalName);
                ScheduleActivity("Sync", "2.1").AfterActivity<TestActivity>(_positionalName);
            }
        }
        private class WorkflowWithMultipleParents : Workflow
        {
            public WorkflowWithMultipleParents()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion);
                ScheduleActivity("Transcode", "2.0").AfterActivity(_activityName, _activityVersion, _positionalName).AfterActivity(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class WorkflowForSchedulableWorkflowActionWithMultipleParents : Workflow
        {
            public WorkflowForSchedulableWorkflowActionWithMultipleParents(string workflowActionResult)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion);
                ScheduleAction((i)=>CompleteWorkflow(workflowActionResult)).AfterActivity(_activityName, _activityVersion, _positionalName).AfterActivity(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class WorkflowWithAParentContinueOnFailure : Workflow
        {
            public WorkflowWithAParentContinueOnFailure()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion).OnFailure(Continue);
                ScheduleActivity("Transcode", "2.0").AfterActivity(_activityName, _activityVersion, _positionalName).AfterActivity(_siblingActivityName, _siblingActivityVersion);
            }
        }
        private class WorkflowWithAParentContinueOnTimedout : Workflow
        {
            public WorkflowWithAParentContinueOnTimedout()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion).OnTimedout(Continue);
                ScheduleActivity("Transcode", "2.0").AfterActivity(_activityName, _activityVersion, _positionalName).AfterActivity(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class WorkflowWithAParentContinueOnCancelled : Workflow
        {
            public WorkflowWithAParentContinueOnCancelled()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion).OnCancelled(Continue);
                ScheduleActivity("Transcode", "2.0").AfterActivity(_activityName, _activityVersion, _positionalName).AfterActivity(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class WorkflowWithAParentIgnoringCompleteEvent : Workflow
        {
            public WorkflowWithAParentIgnoringCompleteEvent()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleActivity(_siblingActivityName, _siblingActivityVersion).OnCompletion(e => Ignore);
                ScheduleActivity("Transcode", "2.0").AfterActivity(_activityName, _activityVersion, _positionalName).AfterActivity(_siblingActivityName, _siblingActivityVersion);
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
                ScheduleTimer(childTimer).AfterTimer(timerName).FireAfter(childTimeout);
            }
        }
        private class WorkflowWithChildActivity : Workflow
        {
            public WorkflowWithChildActivity(string timerName)
            {
                ScheduleTimer(timerName);
                ScheduleActivity(_activityName, _activityVersion).AfterTimer(timerName);
            }
        }
        private class WorkflowWithParentActivityAndChildTimers : Workflow
        {
            public WorkflowWithParentActivityAndChildTimers(string timerName)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);
                ScheduleTimer(timerName).AfterActivity(_activityName, _activityVersion,_positionalName);
            }
        }

        [ActivityDescription(_activityVersion, Name = _activityName)]
        private class TestActivity : Activity
        {
        }
    }
}