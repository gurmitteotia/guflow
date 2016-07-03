using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ActivityItemTests
    {
        private readonly Identity _activityIdenity = Identity.New("somename", "1.0", "name");
        [Test]
        public void By_default_workflow_input_is_passed_as_activity_input()
        {
            const string workflowInput = "actvity";
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(new WorkflowHistoryEvents(HistoryEventFactory.CreateWorkflowStartedEventGraph(workflowInput))));

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Input, Is.EqualTo(workflowInput));
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_input_string()
        {
            const string activityInput = "actvity";
            var activityItem = new ActivityItem(_activityIdenity, null);
            activityItem.WithInput(a => activityInput);

            var decision = (ScheduleActivityDecision) activityItem.GetScheduleDecision();

            Assert.That(decision.Input,Is.EqualTo(activityInput));
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_input_object()
        {
            var activityItem = new ActivityItem(_activityIdenity, null);
            activityItem.WithInput(a => new{InputFile ="SomeFile", Rate =50});

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Input, Is.EqualTo(new { InputFile = "SomeFile", Rate = 50 }.ToJson()));
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_null_input()
        {
            var activityItem = new ActivityItem(_activityIdenity, null);
            activityItem.WithInput(a => null);

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Input, Is.Null);
        }

        [Test]
        public void By_default_schedule_activity_with_empty_task_list()
        {
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(null));

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskList, Is.Null);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_task_list()
        {
            const string taskList = "taskList";
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(null));
            activityItem.OnTaskList(a => taskList);

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskList, Is.EqualTo(taskList));
        }

        [Test]
        public void Does_not_schedule_activity_when_when_func_is_evaluated_to_false()
        {
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(null));
            activityItem.When(a => false);

            var decision = activityItem.GetScheduleDecision();

            Assert.That(decision, Is.EqualTo(WorkflowDecision.Empty));
        }

        [Test]
        public void By_default_schedule_activity_without_priority()
        {
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(null));

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskPriority.HasValue, Is.False);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_priority()
        {
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(null));
            activityItem.WithPriority(a => 10);
            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskPriority.Value, Is.EqualTo(10));
        }

        [Test]
        public void By_default_schedule_activity_with_empty_timeouts()
        {
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(null));

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Timeouts.HeartbeatTimeout, Is.Null);
            Assert.That(decision.Timeouts.ScheduleToCloseTimeout,Is.Null);
            Assert.That(decision.Timeouts.ScheduleToStartTimeout,Is.Null);
            Assert.That(decision.Timeouts.StartToCloseTimeout,Is.Null);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_timeouts()
        {
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(null));
            activityItem.WithTimeouts(
                a =>
                    new ScheduleActivityTimeouts()
                    {
                        HeartbeatTimeout = TimeSpan.FromSeconds(2),
                        ScheduleToCloseTimeout = TimeSpan.FromSeconds(3),
                        ScheduleToStartTimeout = TimeSpan.FromSeconds(4),
                        StartToCloseTimeout = TimeSpan.FromSeconds(5)
                    });
            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Timeouts.HeartbeatTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(decision.Timeouts.ScheduleToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(decision.Timeouts.ScheduleToStartTimeout, Is.EqualTo(TimeSpan.FromSeconds(4)));
            Assert.That(decision.Timeouts.StartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Parent_activities_test()
        {
            var workflowWithParentActivity = new WorkflowWithParentActivity("parent1","1.0","pos");
            var childActivity = new ActivityItem(Identity.New("child","1.0"),workflowWithParentActivity);
            childActivity.After("parent1", "1.0","pos");

            var parentActivities = childActivity.ParentActivities;
            
            Assert.That(parentActivities,Is.EquivalentTo(new []{new ActivityItem(Identity.New("parent1","1.0","pos"),null)}));
            Assert.That(parentActivities.First().Name, Is.EqualTo("parent1"));
            Assert.That(parentActivities.First().Version, Is.EqualTo("1.0"));
            Assert.That(parentActivities.First().PositionalName, Is.EqualTo("pos"));
        }

        [Test]
        public void Parent_timers_test()
        {
            var workflowWithParentActivity = new WorkflowWithParentTimer("parent1");
            var childActivity = new ActivityItem(Identity.New("child", "1.0"), workflowWithParentActivity);
            childActivity.After("parent1");

            var parentActivities = childActivity.ParentTimers;

            Assert.That(parentActivities, Is.EquivalentTo(new[] { new TimerItem(Identity.Timer("parent1"), null),  }));
            Assert.That(parentActivities.First().Name, Is.EqualTo("parent1"));
        }

        [Test]
        public void Last_event_can_be_activity_started_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id");
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);
            var activityItem = new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(workflowHistoryEvents));

            var latestEvent = activityItem.LastEvent;

            Assert.That(latestEvent,Is.EqualTo(new ActivityStartedEvent(eventGraph.First(),eventGraph)));
        }

        [Test]
        public void Last_event_can_be_activity_scheduled_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityScheduledEventGraph(_activityIdenity);
            var activityItem = CreateActivityItemWith(eventGraph);

            var latestEvent = activityItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new ActivityScheduledEvent(eventGraph.First(),eventGraph)));
        }

        [Test]
        public void Last_event_can_be_activity_cancel_requested_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityCancelRequestedGraph(_activityIdenity, "workerid");
            var activityItem = CreateActivityItemWith(eventGraph);

            var latestEvent = activityItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new ActivityCancelRequestedEvent(eventGraph.First())));
        }

        [Test]
        public void Last_event_can_be_activity_cancellation_failed_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityCancellationFailedEventGraph(_activityIdenity, "workerid");
            var activityItem = CreateActivityItemWith(eventGraph);
            
            var latestEvent = activityItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new ActivityCancellationFailedEvent(eventGraph.First())) );
        }

        [Test]
        public void Last_event_can_be_activity_scheduling_failed_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivitySchedulingFailedEventGraph(_activityIdenity, "workerid");
            var activityItem = CreateActivityItemWith(eventGraph);

            var latestEvent = activityItem.LastEvent;

            Assert.That(latestEvent,Is.EqualTo(new ActivitySchedulingFailedEvent(eventGraph.First())));
        }

        [Test]
        public void Last_event_is_timer_event_when_timer_events_are_newer_then_activity_event()
        {
            var timerStartedEventGraph = HistoryEventFactory.CreateTimerStartedEventGraph(_activityIdenity,TimeSpan.FromSeconds(2));
            var eventGraph = timerStartedEventGraph.Concat(HistoryEventFactory.CreateActivityFailedEventGraph(_activityIdenity, "workerid","reason","detail"));
            var activityItem = CreateActivityItemWith(eventGraph);

            var latestEvent = activityItem.LastEvent;

            Assert.That(latestEvent,Is.EqualTo(new TimerStartedEvent(timerStartedEventGraph.First(),timerStartedEventGraph)));
        }

        [Test]
        public void Last_event_is_activity_event_when_activity_events_are_newer_then_timer_event()
        {
            var activityFailedEventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(_activityIdenity, "workerid", "reason", "detail");
            var eventGraph =activityFailedEventGraph.Concat(HistoryEventFactory.CreateTimerFiredEventGraph(_activityIdenity,TimeSpan.FromSeconds(2)));
            var activityItem = CreateActivityItemWith(eventGraph);


            var latestEvent = activityItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new ActivityFailedEvent(activityFailedEventGraph.First(), activityFailedEventGraph)));
        }

        [Test]
        public void Can_return_last_completed_event()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityIdenity, "workerid", "detail");
            var eventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(_activityIdenity, "id","reason","detail").Concat(activityCompletedEventGraph);
            var activityItem = CreateActivityItemWith(eventGraph);

            var lastCompletedEvent = activityItem.LastCompletedEvent;

            Assert.That(lastCompletedEvent, Is.EqualTo(new ActivityCompletedEvent(activityCompletedEventGraph.First(), activityCompletedEventGraph)));
        }
        [Test]
        public void Can_return_null_when_last_completed_event_is_not_found()
        {
            var eventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(_activityIdenity, "id", "reason", "detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var lastCompletedEvent = activityItem.LastCompletedEvent;

            Assert.That(lastCompletedEvent, Is.Null);
        }
        [Test]
        public void Can_return_last_failed_event()
        {
            var activityFailedEventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(_activityIdenity, "workerid", "reason","detail");
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id").Concat(activityFailedEventGraph);
            var activityItem = CreateActivityItemWith(eventGraph);

            var lastFailedEvent = activityItem.LastFailedEvent;

            Assert.That(lastFailedEvent, Is.EqualTo(new ActivityFailedEvent(activityFailedEventGraph.First(), activityFailedEventGraph)));
        }
        [Test]
        public void Can_return_null_when_last_failed_event_is_not_found()
        {
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id");
            var activityItem = CreateActivityItemWith(eventGraph);

            var lastFailedEvent = activityItem.LastFailedEvent;

            Assert.That(lastFailedEvent, Is.Null);
        }
        [Test]
        public void Can_return_last_timedout_event()
        {
            var activityTimedoutEventGraph = HistoryEventFactory.CreateActivityTimedoutEventGraph(_activityIdenity, "workerid", "reason", "detail");
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id").Concat(activityTimedoutEventGraph);
            var activityItem = CreateActivityItemWith(eventGraph);

            var lastTimedoutEvent = activityItem.LastTimedoutEvent;

            Assert.That(lastTimedoutEvent, Is.EqualTo(new ActivityTimedoutEvent(activityTimedoutEventGraph.First(), activityTimedoutEventGraph)));
        }
        [Test]
        public void Can_return_null_when_last_timedout_event_is_not_found()
        {
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id");
            var activityItem = CreateActivityItemWith(eventGraph);

            var lastTimedoutEvent = activityItem.LastTimedoutEvent;

            Assert.That(lastTimedoutEvent, Is.Null);
        }
        [Test]
        public void Can_return_last_cancelled_event()
        {
            var activityCancelledEventGraph = HistoryEventFactory.CreateActivityCancelledEventGraph(_activityIdenity, "workerid", "detail");
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id").Concat(activityCancelledEventGraph);
            var activityItem = CreateActivityItemWith(eventGraph);

            var lastCancelledEvent = activityItem.LastCancelledEvent;

            Assert.That(lastCancelledEvent, Is.EqualTo(new ActivityCancelledEvent(activityCancelledEventGraph.First(), activityCancelledEventGraph)));
        }
        [Test]
        public void Can_return_null_when_last_cancelled_event_is_not_found()
        {
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id");
            var activityItem = CreateActivityItemWith(eventGraph);

            var lastCancelledEvent = activityItem.LastCancelledEvent;

            Assert.That(lastCancelledEvent, Is.Null);
        }

        [Test]
        public void All_events_can_return_completed_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityIdenity, "workerid", "detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[]{new ActivityCompletedEvent(eventGraph.First(), eventGraph)}));
        }

        [Test]
        public void All_events_can_return_completed_event_and_started_event()
        {
            var completedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityIdenity, "workerid", "detail");
            var startedEventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id");
            var activityItem = CreateActivityItemWith(completedEventGraph.Concat(startedEventGraph));

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new WorkflowItemEvent[] { new ActivityCompletedEvent(completedEventGraph.First(), completedEventGraph), new ActivityStartedEvent(startedEventGraph.First(), startedEventGraph) }));
        }

        [Test]
        public void All_events_can_return_completed_event_and_scheduled_event()
        {
            var completedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityIdenity, "workerid", "detail");
            var scheduledEventGraph = HistoryEventFactory.CreateActivityScheduledEventGraph(_activityIdenity);
            var activityItem = CreateActivityItemWith(completedEventGraph.Concat(scheduledEventGraph));

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new WorkflowItemEvent[] { new ActivityCompletedEvent(completedEventGraph.First(), completedEventGraph), new ActivityScheduledEvent(scheduledEventGraph.First(), scheduledEventGraph),  }));
        }

        [Test]
        public void All_events_can_return_failed_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(_activityIdenity, "workerid", "reason","detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivityFailedEvent(eventGraph.First(), eventGraph),  }));
        }

        [Test]
        public void All_events_can_return_timedout_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityTimedoutEventGraph(_activityIdenity, "workerid", "reason", "detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivityTimedoutEvent(eventGraph.First(), eventGraph), }));
        }

        [Test]
        public void All_events_can_return_cancelled_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityCancelledEventGraph(_activityIdenity, "workerid", "detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivityCancelledEvent(eventGraph.First(), eventGraph), }));
        }

        [Test]
        public void All_events_can_return_cancelled_event_and_cancel_requested_event()
        {
            var cancelledEventGraph = HistoryEventFactory.CreateActivityCancelledEventGraph(_activityIdenity, "workerid", "detail");
            var cancelRequestedEventGraph = HistoryEventFactory.CreateActivityCancelRequestedGraph(_activityIdenity,"id");
            var activityItem = CreateActivityItemWith(cancelledEventGraph.Concat(cancelRequestedEventGraph));

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new WorkflowItemEvent[] { new ActivityCancelledEvent(cancelledEventGraph.First(), cancelledEventGraph), new ActivityCancelRequestedEvent(cancelRequestedEventGraph.First()),  }));
        }

        [Test]
        public void All_events_can_return_cancellation_failed_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityCancellationFailedEventGraph(_activityIdenity, "cause");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivityCancellationFailedEvent(eventGraph.First()), }));
        }

        [Test]
        public void All_events_can_return_cancelled_event_and_cancellation_request_failed_event()
        {
            var cancelledEventGraph = HistoryEventFactory.CreateActivityCancelledEventGraph(_activityIdenity, "workerid", "detail");
            var activityCancellationFailedEventGraph = HistoryEventFactory.CreateActivityCancellationFailedEventGraph(_activityIdenity, "id");
            var activityItem = CreateActivityItemWith(cancelledEventGraph.Concat(activityCancellationFailedEventGraph));

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new WorkflowItemEvent[] { new ActivityCancelledEvent(cancelledEventGraph.First(), cancelledEventGraph), new ActivityCancellationFailedEvent(activityCancellationFailedEventGraph.First()), }));
        }

        [Test]
        public void All_events_can_return_activity_started_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "workerid");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new []{ new ActivityStartedEvent(eventGraph.First(),eventGraph)}));
        }

        [Test]
        public void All_events_can_return_activity_scheduled_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivityScheduledEventGraph(_activityIdenity);
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivityScheduledEvent(eventGraph.First(), eventGraph),  }));
        }

        [Test]
        public void All_events_can_return_activity_scheduling_failed_event()
        {
            var eventGraph = HistoryEventFactory.CreateActivitySchedulingFailedEventGraph(_activityIdenity,"cause");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivitySchedulingFailedEvent(eventGraph.First()), }));
        }

        [Test]
        public void All_events_can_return_timer_fired_event()
        {
            var eventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(_activityIdenity, TimeSpan.FromSeconds(3),true);
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new TimerFiredEvent(eventGraph.First(),eventGraph)}));
        }

        [Test]
        public void All_events_return_the_events_in_the_order_of_their_occurrence()
        {
            var startedEventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id");
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(_activityIdenity, TimeSpan.FromSeconds(3), true);
            var completedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityIdenity, "workerid", "detail");

            var activityItem = CreateActivityItemWith(startedEventGraph.Concat(timerFiredEventGraph).Concat(completedEventGraph));

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[] {new ActivityStartedEvent(startedEventGraph.First(),startedEventGraph) ,
                                                                       new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph),
                                                                       new ActivityCompletedEvent(completedEventGraph.First(),completedEventGraph)}));
        }

        [Test]
        public void All_events_can_return_timer_fired_and_timer_started_event_event()
        {
            var timerStartedEventGraph = HistoryEventFactory.CreateTimerStartedEventGraph(_activityIdenity,TimeSpan.FromSeconds(3),true);
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(_activityIdenity, TimeSpan.FromSeconds(3), true);
            var activityItem = CreateActivityItemWith(timerStartedEventGraph.Concat(timerFiredEventGraph));

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new TimerEvent[] {new TimerStartedEvent(timerStartedEventGraph.First(),timerStartedEventGraph), new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph)}));
        }

        [Test]
        public void All_events_can_return_timer_cancelled_event()
        {
            var eventGraph = HistoryEventFactory.CreateTimerCancelledEventGraph(_activityIdenity, TimeSpan.FromSeconds(3), true);
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new TimerCancelledEvent(eventGraph.First(), eventGraph),  }));
        }

        [Test]
        public void All_events_can_return_timer_cancelled_and_timer_started_event()
        {
            var timerStartedEventGraph = HistoryEventFactory.CreateTimerStartedEventGraph(_activityIdenity,TimeSpan.FromSeconds(3),true);
            var timerCancelledEventGraph = HistoryEventFactory.CreateTimerCancelledEventGraph(_activityIdenity, TimeSpan.FromSeconds(3), true);
            var activityItem = CreateActivityItemWith(timerStartedEventGraph.Concat(timerCancelledEventGraph));

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo( new TimerEvent[] { new TimerStartedEvent(timerStartedEventGraph.First(), timerStartedEventGraph), new TimerCancelledEvent(timerCancelledEventGraph.First(), timerCancelledEventGraph), }));
        }

        [Test]
        public void All_events_can_return_timer_cancellattion_failed_event()
        {
            var eventGraph = HistoryEventFactory.CreateTimerCancellationFailedEventGraph(_activityIdenity, "cause");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new[] { new TimerCancellationFailedEvent(eventGraph.First())}));
        }

        [Test]
        public void All_events_can_return_timer_started_and_cancellattion_failed_event()
        {
            var timerStartedEventGraph = HistoryEventFactory.CreateTimerStartedEventGraph(_activityIdenity, TimeSpan.FromSeconds(3), true);
            var timerCancellationFailedEventGraph = HistoryEventFactory.CreateTimerCancellationFailedEventGraph(_activityIdenity, "cause");
            var activityItem = CreateActivityItemWith(timerStartedEventGraph.Concat(timerCancellationFailedEventGraph));

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new WorkflowItemEvent[] { new TimerStartedEvent(timerStartedEventGraph.First(), timerStartedEventGraph), new TimerCancellationFailedEvent(timerCancellationFailedEventGraph.First()) }));
        }

        [Test]
        public void All_events_can_return_timer_started_event()
        {
            var eventGraph = HistoryEventFactory.CreateTimerStartedEventGraph(_activityIdenity, TimeSpan.FromSeconds(3), true);
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new [] { new TimerStartedEvent(eventGraph.First(), eventGraph)}));
        }

        [Test]
        public void All_events_can_return_timer_start_failed_event()
        {
            var eventGraph = HistoryEventFactory.CreateTimerStartFailedEventGraph(_activityIdenity,"cause");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents;

            Assert.That(allEvents, Is.EquivalentTo(new [] { new TimerStartFailedEvent(eventGraph.First()) }));
        }

        [Test]
        public void Should_be_active_when_last_event_is_active()
        {
            var startedEventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(_activityIdenity, "id");
            var activityItem = CreateActivityItemWith(startedEventGraph);
            Assert.IsTrue(activityItem.IsActive);
        }

        [Test]
        public void Should_not_be_active_when_no_event_is_found()
        {
            var activityItem = CreateActivityItemWith(HistoryEventFactory.CreateActivityStartedEventGraph(Identity.New("Different",""),"id"));
            Assert.IsFalse(activityItem.IsActive);
        }

        private ActivityItem CreateActivityItemWith(IEnumerable<HistoryEvent> eventGraph)
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);
            return new ActivityItem(_activityIdenity, new WorkflowToStubHistoryEvents(workflowHistoryEvents));
        }
       
        private class WorkflowWithParentActivity : Workflow
        {
            public WorkflowWithParentActivity(string parentActivityName,string parentActivityVersion, string postionalName)
            {
                ScheduleActivity(parentActivityName, parentActivityVersion, postionalName);
            }
        }
        private class WorkflowWithParentTimer : Workflow
        {
            public WorkflowWithParentTimer(string parentTimerName)
            {
                ScheduleTimer(parentTimerName);
            }
        }
    }
}