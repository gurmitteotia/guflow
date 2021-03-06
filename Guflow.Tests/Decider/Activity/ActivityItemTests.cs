﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
    public class ActivityItemTests
    {
        private readonly Identity _activityIdentity = Identity.New("somename", "1.0", "name");
        private ScheduleId _scheduleId;
        private Mock<IWorkflow> _workflow;

        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(_eventsBuilder.Result);
            _scheduleId =  _activityIdentity.ScheduleId();
        }
        [Test]
        public void By_default_workflow_input_is_passed_as_activity_input()
        {
            const string workflowInput = "actvity";
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_eventGraphBuilder.WorkflowStartedGraph(workflowInput)));
            var activityItem = new ActivityItem(_activityIdentity,_workflow.Object);

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.Input, Is.EqualTo(workflowInput));
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_input_string()
        {
            const string activityInput = "actvity";
            var activityItem = new ActivityItem(_activityIdentity, null);
            activityItem.WithInput(a => activityInput);

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.Input,Is.EqualTo(activityInput));
        }
        [Test]
        public void Can_be_configured_to_schedule_activity_with_primitive_string()
        {
            DateTime activityInput = DateTime.Now;
            var activityItem = new ActivityItem(_activityIdentity, null);
            activityItem.WithInput(a => activityInput);

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.Input, Is.EqualTo(activityInput.ToString()));
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_input_object()
        {
            var activityItem = new ActivityItem(_activityIdentity, null);
            activityItem.WithInput(a => new{InputFile ="SomeFile", Rate =50});

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.Input, Is.EqualTo(new { InputFile = "SomeFile", Rate = 50 }.ToJson()));
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_null_input()
        {
            var activityItem = new ActivityItem(_activityIdentity, null);
            activityItem.WithInput(a => null);

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.Input, Is.Null);
        }

        [Test]
        public void By_default_schedule_activity_with_empty_task_list()
        {
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.TaskListName, Is.Null);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_task_list()
        {
            const string taskList = "taskList";
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);
            activityItem.OnTaskList(a => taskList);

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.TaskListName, Is.EqualTo(taskList));
        }

        [Test]
        public void Does_not_schedule_activity_when_when_func_is_evaluated_to_false()
        {
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);
            activityItem.When(a => false);

            var decisions = activityItem.ScheduleDecisions();

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void By_default_schedule_activity_without_priority()
        {
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.TaskPriority.HasValue, Is.False);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_priority()
        {
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);
            activityItem.WithPriority(a => 10);
            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.TaskPriority.Value, Is.EqualTo(10));
        }

        [Test]
        public void By_default_schedule_activity_with_empty_timeouts()
        {
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);

            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.Timeouts.HeartbeatTimeout, Is.Null);
            Assert.That(decision.Timeouts.ScheduleToCloseTimeout,Is.Null);
            Assert.That(decision.Timeouts.ScheduleToStartTimeout,Is.Null);
            Assert.That(decision.Timeouts.StartToCloseTimeout,Is.Null);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_timeouts()
        {
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);
            activityItem.WithTimeouts(
                a =>
                    new ActivityTimeouts()
                    {
                        HeartbeatTimeout = TimeSpan.FromSeconds(2),
                        ScheduleToCloseTimeout = TimeSpan.FromSeconds(3),
                        ScheduleToStartTimeout = TimeSpan.FromSeconds(4),
                        StartToCloseTimeout = TimeSpan.FromSeconds(5)
                    });
            var decision = ScheduleDecision(activityItem);

            Assert.That(decision.Timeouts.HeartbeatTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(decision.Timeouts.ScheduleToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(decision.Timeouts.ScheduleToStartTimeout, Is.EqualTo(TimeSpan.FromSeconds(4)));
            Assert.That(decision.Timeouts.StartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Last_event_can_be_null()
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(_eventGraphBuilder.WorkflowStartedGraph());
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(workflowHistoryEvents);
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);

            var latestEvent = activityItem.LastEvent();

            Assert.IsNull(latestEvent);
        }

        [Test]
        public void Last_event_is_timer_event_when_timer_events_are_newer_then_activity_event()
        {
            var activityFailedEventGraph = _eventGraphBuilder.ActivityFailedGraph(_scheduleId, "workerid", "reason", "detail");
            var timerStartedEventGraph = _eventGraphBuilder.TimerStartedGraph(_scheduleId, TimeSpan.FromSeconds(2));
            
            var activityItem = CreateActivityItemWith(activityFailedEventGraph.Concat(timerStartedEventGraph));

            var latestEvent = activityItem.LastEvent(true);

            Assert.That(latestEvent,Is.EqualTo(new TimerStartedEvent(timerStartedEventGraph.First(),timerStartedEventGraph)));
        }

        [Test]
        public void Last_event_is_activity_event_when_activity_events_are_newer_then_timer_event()
        {
            var timerFiredEventGraph = _eventGraphBuilder.TimerFiredGraph(_scheduleId, TimeSpan.FromSeconds(2));
            var activityFailedEventGraph = _eventGraphBuilder.ActivityFailedGraph(_scheduleId, "workerid", "reason", "detail");
            var eventGraph = timerFiredEventGraph.Concat(activityFailedEventGraph);
            var activityItem = CreateActivityItemWith(eventGraph);


            var latestEvent = activityItem.LastEvent(true);

            Assert.That(latestEvent, Is.EqualTo(new ActivityFailedEvent(activityFailedEventGraph.First(), activityFailedEventGraph)));
        }

        [Test]
        public void Last_event_is_activity_started_event_when_its_cancel_request_is_in_progress()
        {
            var eventGraph = _eventGraphBuilder.ActivityCancelRequestedGraph(_scheduleId, "id").ToArray();
            var activityItem = CreateActivityItemWith(eventGraph);


            var last = activityItem.LastEvent();

            Assert.That(last, Is.EqualTo(new ActivityStartedEvent(eventGraph.Skip(1).First(), eventGraph)));
        }

        [Test]
        public void Last_event_is_activity_started_event_when_its_cancel_request_is_failed()
        {
            var eventGraph = _eventGraphBuilder.ActivityCancellationFailedGraph(_scheduleId, "cause").ToArray();
            var activityItem = CreateActivityItemWith(eventGraph);


            var last = activityItem.LastEvent();

            Assert.That(last, Is.EqualTo(new ActivityStartedEvent(eventGraph.Skip(1).First(), eventGraph)));
        }

        [Test]
        public void Last_event_filters_out_activity_scheduling_failed_event()
        {
            var activityScheduled = _eventGraphBuilder.ActivityScheduledGraph(_scheduleId);
            var activityScheduleFailed = _eventGraphBuilder.ActivitySchedulingFailedGraph(_scheduleId, "DUPLICATE_ID");
            var activityItem = CreateActivityItemWith(activityScheduleFailed.Concat(activityScheduled));

            var @event = activityItem.LastEvent();
            Assert.That(@event, Is.EqualTo(new ActivityScheduledEvent(activityScheduled.First(), activityScheduled)));
        }

        [Test]
        public void Last_event_by_default_filter_out_reschedule_timer_events()
        {
            var activityFailedEventGraph = _eventGraphBuilder.ActivityFailedGraph(_scheduleId, "workerid", "reason", "detail");
            var timerStartedEventGraph = _eventGraphBuilder.TimerStartedGraph(_scheduleId, TimeSpan.FromSeconds(2));

            var activityItem = CreateActivityItemWith(activityFailedEventGraph.Concat(timerStartedEventGraph));

            var latestEvent = activityItem.LastEvent();

            Assert.That(latestEvent, Is.EqualTo(new ActivityFailedEvent(activityFailedEventGraph.First(), activityFailedEventGraph)));
        }

        [Test]
        public void By_default_last_similar_events_is_empty()
        {
            var activityItem = new ActivityItem(_activityIdentity, _workflow.Object);

            Assert.That(activityItem.LastSimilarEvents(), Is.Empty);
        }

        [Test]
        public void All_events_can_return_completed_event()
        {
            var eventGraph = _eventGraphBuilder.ActivityCompletedGraph(_scheduleId, "workerid", "detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new[]{new ActivityCompletedEvent(eventGraph.First(), eventGraph)}));
        }

        [Test]
        public void All_events_can_return_completed_event_and_started_event()
        {
            var completedEventGraph = _eventGraphBuilder.ActivityCompletedGraph(_scheduleId, "workerid", "detail");
            var startedEventGraph = _eventGraphBuilder.ActivityStartedGraph(_scheduleId, "id");
            var activityItem = CreateActivityItemWith(completedEventGraph.Concat(startedEventGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new WorkflowItemEvent[] { new ActivityCompletedEvent(completedEventGraph.First(), completedEventGraph), new ActivityStartedEvent(startedEventGraph.First(), startedEventGraph) }));
        }

        [Test]
        public void All_events_can_return_completed_event_and_scheduled_event()
        {
            var completedEventGraph = _eventGraphBuilder.ActivityCompletedGraph(_scheduleId, "workerid", "detail");
            var scheduledEventGraph = _eventGraphBuilder.ActivityScheduledGraph(_scheduleId);
            var activityItem = CreateActivityItemWith(completedEventGraph.Concat(scheduledEventGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new WorkflowItemEvent[] { new ActivityCompletedEvent(completedEventGraph.First(), completedEventGraph), new ActivityScheduledEvent(scheduledEventGraph.First(), scheduledEventGraph),  }));
        }

        [Test]
        public void All_events_can_return_failed_event()
        {
            var eventGraph = _eventGraphBuilder.ActivityFailedGraph(_scheduleId, "workerid", "reason","detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivityFailedEvent(eventGraph.First(), eventGraph),  }));
        }

        [Test]
        public void All_events_can_return_timedout_event()
        {
            var eventGraph = _eventGraphBuilder.ActivityTimedoutGraph(_scheduleId, "workerid", "reason", "detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivityTimedoutEvent(eventGraph.First(), eventGraph), }));
        }

        [Test]
        public void All_events_can_return_cancelled_event_and_cancel_requested_event()
        {
            var eventGraph = _eventGraphBuilder.ActivityCancelledGraph(_scheduleId, "workerid", "detail");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ActivityCancelledEvent(eventGraph.First(), eventGraph),
                new ActivityCancelRequestedEvent(eventGraph.Skip(1).First()), 
            }));
        }

        [Test]
        public void All_events_can_return_multiple_cancel_requested_event_from_different_graph()
        {
            var cancelledEventGraph = _eventGraphBuilder.ActivityCancelledGraph(_scheduleId, "workerid", "detail").ToArray();
            var cancelRequestedEventGraph = _eventGraphBuilder.ActivityCancelRequestedGraph(_scheduleId, "id").ToArray();
            var activityItem = CreateActivityItemWith(cancelRequestedEventGraph.Concat(cancelledEventGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ActivityCancelRequestedEvent(cancelRequestedEventGraph.First()),
                new ActivityStartedEvent(cancelRequestedEventGraph.Skip(1).First(), cancelRequestedEventGraph),
                new ActivityCancelledEvent(cancelledEventGraph.First(), cancelledEventGraph),
                new ActivityCancelRequestedEvent(cancelledEventGraph.Skip(1).First()), 
            }));
        }

        [Test]
        public void All_events_can_return_cancellation_failed_event_and_activity_started_event()
        {
            var eventGraph = _eventGraphBuilder.ActivityCancellationFailedGraph(_scheduleId, "cause").ToArray();
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ActivityCancellationFailedEvent(eventGraph.First()),
                new ActivityStartedEvent(eventGraph.Skip(1).First(), eventGraph)
            }));
        }

        [Test]
        public void All_events_can_return_cancelled_event_and_cancellation_request_failed_event()
        {
            var cancelledEventGraph = _eventGraphBuilder.ActivityCancelledGraph(_scheduleId, "workerid", "detail").ToArray();
            var activityCancellationFailedEventGraph = _eventGraphBuilder.ActivityCancellationFailedGraph(_scheduleId, "id").ToArray();
            var activityItem = CreateActivityItemWith(activityCancellationFailedEventGraph.Concat(cancelledEventGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ActivityCancellationFailedEvent(activityCancellationFailedEventGraph.First()),
                new ActivityStartedEvent(activityCancellationFailedEventGraph.Skip(1).First(), activityCancellationFailedEventGraph),
                new ActivityCancelledEvent(cancelledEventGraph.First(), cancelledEventGraph),
                new ActivityCancelRequestedEvent(cancelledEventGraph.Skip(1).First()), 
            }));
        }

        [Test]
        public void All_events_can_return_activity_started_event()
        {
            var eventGraph = _eventGraphBuilder.ActivityStartedGraph(_scheduleId, "workerid");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new []{ new ActivityStartedEvent(eventGraph.First(),eventGraph)}));
        }

        [Test]
        public void All_events_can_return_activity_scheduled_event()
        {
            var eventGraph = _eventGraphBuilder.ActivityScheduledGraph(_scheduleId);
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivityScheduledEvent(eventGraph.First(), eventGraph),  }));
        }

        [Test]
        public void All_events_can_return_activity_scheduling_failed_event()
        {
            var eventGraph = _eventGraphBuilder.ActivitySchedulingFailedGraph(_scheduleId, "cause");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new[] { new ActivitySchedulingFailedEvent(eventGraph.First()), }));
        }

        [Test]
        public void All_events_can_return_timer_fired_event()
        {
            var eventGraph = _eventGraphBuilder.TimerFiredGraph(_scheduleId, TimeSpan.FromSeconds(3),true);
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new[] { new TimerFiredEvent(eventGraph.First(),eventGraph)}));
        }

        [Test]
        public void All_events_return_the_events_in_the_order_of_their_occurrence()
        {
            var startedEventGraph = _eventGraphBuilder.ActivityStartedGraph(_scheduleId, "id");
            var timerFiredEventGraph = _eventGraphBuilder.TimerFiredGraph(_scheduleId, TimeSpan.FromSeconds(3), true);
            var completedEventGraph = _eventGraphBuilder.ActivityCompletedGraph(_scheduleId, "workerid", "detail");

            var activityItem = CreateActivityItemWith(startedEventGraph.Concat(timerFiredEventGraph).Concat(completedEventGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[] {new ActivityCompletedEvent(completedEventGraph.First(),completedEventGraph),
                                                                        new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph),
                                                                        new ActivityStartedEvent(startedEventGraph.First(),startedEventGraph),
                                                                       }));
        }

        [Test]
        public void All_events_by_default_filters_out_reschedule_timer_events()
        {
            var startedEventGraph = _eventGraphBuilder.ActivityStartedGraph(_scheduleId, "id");
            var timerFiredEventGraph = _eventGraphBuilder.TimerFiredGraph(_scheduleId, TimeSpan.FromSeconds(3), true);
            var completedEventGraph = _eventGraphBuilder.ActivityCompletedGraph(_scheduleId, "workerid", "detail");

            var activityItem = CreateActivityItemWith(startedEventGraph.Concat(timerFiredEventGraph).Concat(completedEventGraph));

            var allEvents = activityItem.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[] {new ActivityCompletedEvent(completedEventGraph.First(),completedEventGraph),
                new ActivityStartedEvent(startedEventGraph.First(),startedEventGraph),
            }));
        }

        [Test]
        public void All_events_can_return_timer_fired_and_timer_started_event_event()
        {
            var timerStartedEventGraph = _eventGraphBuilder.TimerStartedGraph(_scheduleId,TimeSpan.FromSeconds(3),true);
            var timerFiredEventGraph = _eventGraphBuilder.TimerFiredGraph(_scheduleId, TimeSpan.FromSeconds(3), true);
            var activityItem = CreateActivityItemWith(timerStartedEventGraph.Concat(timerFiredEventGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new TimerEvent[] {new TimerStartedEvent(timerStartedEventGraph.First(),timerStartedEventGraph), 
                                            new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph)}));
        }

        [Test]
        public void All_events_can_return_timer_cancelled_event()
        {
            var eventGraph = _eventGraphBuilder.TimerCancelledGraph(_scheduleId, TimeSpan.FromSeconds(3), true);
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new[] { new TimerCancelledEvent(eventGraph.First(), eventGraph),  }));
        }

        [Test]
        public void All_events_can_return_timer_cancelled_and_timer_started_event()
        {
            var timerStartedEventGraph = _eventGraphBuilder.TimerStartedGraph(_scheduleId,TimeSpan.FromSeconds(3),true);
            var timerCancelledEventGraph = _eventGraphBuilder.TimerCancelledGraph(_scheduleId, TimeSpan.FromSeconds(3), true);
            var activityItem = CreateActivityItemWith(timerStartedEventGraph.Concat(timerCancelledEventGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo( new TimerEvent[] { new TimerStartedEvent(timerStartedEventGraph.First(), timerStartedEventGraph), 
                                new TimerCancelledEvent(timerCancelledEventGraph.First(), timerCancelledEventGraph), }));
        }

        [Test]
        public void All_events_can_return_timer_cancellattion_failed_event_and_timer_started_event()
        {
            var startedGraph = _eventGraphBuilder.TimerStartedGraph(_scheduleId, TimeSpan.Zero);
            var failedGraph = _eventGraphBuilder.TimerCancellationFailedGraph(_scheduleId, "cause");
            var activityItem = CreateActivityItemWith(failedGraph.Concat(startedGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new TimerCancellationFailedEvent(failedGraph.First()),
                new TimerStartedEvent(startedGraph.First(), startedGraph)
            }));
        }

        [Test]
        public void All_events_can_return_timer_fired_and_cancellattion_failed_event()
        {
            var firedGraph = _eventGraphBuilder.TimerFiredGraph(_scheduleId, TimeSpan.Zero);
            var failedGraph = _eventGraphBuilder.TimerCancellationFailedGraph(_scheduleId, "cause");

            var activityItem = CreateActivityItemWith(failedGraph.Concat(firedGraph));

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new TimerCancellationFailedEvent(failedGraph.First()),
                new TimerFiredEvent(firedGraph.First(), firedGraph),
            }));
        }

        [Test]
        public void All_events_can_return_timer_started_event()
        {
            var eventGraph = _eventGraphBuilder.TimerStartedGraph(_scheduleId, TimeSpan.FromSeconds(3), true);
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new [] { new TimerStartedEvent(eventGraph.First(), eventGraph)}));
        }

        [Test]
        public void All_events_can_return_timer_start_failed_event()
        {
            var eventGraph = _eventGraphBuilder.TimerStartFailedGraph(_scheduleId,"cause");
            var activityItem = CreateActivityItemWith(eventGraph);

            var allEvents = activityItem.AllEvents(true);

            Assert.That(allEvents, Is.EquivalentTo(new [] { new TimerStartFailedEvent(eventGraph.First()) }));
        }

        [Test]
        public void Should_be_active_when_last_event_is_activity_started_event()
        {
            var startedEventGraph = _eventGraphBuilder.ActivityStartedGraph(_scheduleId, "id");
            var activityItem = CreateActivityItemWith(startedEventGraph);
            Assert.IsTrue(activityItem.IsActive);
        }

        [Test]
        public void Should_be_active_when_last_event_is_reschedule_timer_started()
        {
            var startedEventGraph = _eventGraphBuilder.TimerStartedGraph(_scheduleId, TimeSpan.Zero, true);
            var activityItem = CreateActivityItemWith(startedEventGraph);
            Assert.IsTrue(activityItem.IsActive);
        }

        [Test]
        public void Should_not_be_active_when_no_event_is_found()
        {
            var activityItem = CreateActivityItemWith(_eventGraphBuilder.ActivityStartedGraph(Identity.New("Different","").ScheduleId(),"id"));
            Assert.IsFalse(activityItem.IsActive);
        }

        [Test]
        public void Invalid_arguments_tests()
        {
            var activityItem = (IFluentActivityItem)new ActivityItem(_activityIdentity, null);

            Assert.Throws<ArgumentNullException>(() => activityItem.WithInput(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.OnCancelled(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.OnCompletion(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.OnCancellationFailed(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.OnSchedulingFailed(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.OnFailure(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.OnTaskList(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.OnTimedout(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.When(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.WithPriority(null));
            Assert.Throws<ArgumentNullException>(() => activityItem.WithTimeouts(null));

            Assert.Throws<ArgumentException>(() => activityItem.AfterActivity(null, "1.0"));
            Assert.Throws<ArgumentException>(() => activityItem.AfterActivity("34", null));
            Assert.Throws<ArgumentException>(() => activityItem.AfterTimer(null));
            Assert.Throws<ArgumentException>(() => activityItem.AfterLambda(null));
            Assert.Throws<ArgumentException>(() => activityItem.AfterChildWorkflow(null,"ver"));
            Assert.Throws<ArgumentException>(() => activityItem.AfterChildWorkflow("name", null));
        }

        private ScheduleActivityDecision ScheduleDecision(ActivityItem activityItem)
        {
            return (ScheduleActivityDecision) activityItem.ScheduleDecisions().Single();
        }
        private ActivityItem CreateActivityItemWith(IEnumerable<HistoryEvent> eventGraph)
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(workflowHistoryEvents);
            return new ActivityItem(_activityIdentity, _workflow.Object);
        }
    }
}