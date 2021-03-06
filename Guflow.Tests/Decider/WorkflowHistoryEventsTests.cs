﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowHistoryEventsTests
    {
        private EventGraphBuilder _builder;

        private const string ActivityName = "Activity1";
        private const string ActivityVersion = "1.0";
        private const string TimerName = "timer1";
        private const string WorkflowName = "Cname";
        private const string WorkflowVersion = "1.0";
        private const string LambdaName = "LambdaName";
        private Identity _childWorkflow;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _childWorkflow = Identity.New("childWorkflow", "ver");
        }


        [Test]
        public void Activity_completed_event_is_interpreted()
        {
            var eventGraph = ActivityCompletedEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new ActivityCompletedEvent(eventGraph.First(), eventGraph) }));
        }
        [Test]
        public void Activity_failed_event_is_interpreted()
        {
            var eventGraph = ActivityFailedEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new ActivityFailedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void Activity_timedout_event_is_interpreted()
        {
            var eventGraph = ActivityTimedoutEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new ActivityTimedoutEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void Activity_cancelled_event_is_interpreted()
        {
            var eventGraph = ActivityCancelledEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new ActivityCancelledEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void Activity_cancellation_failed_event_is_interpreted()
        {
            var eventGraph = ActivityCancellationFailedEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new ActivityCancellationFailedEvent(eventGraph.First()) }));
        }

        [Test]
        public void Workflow_started_event_is_interpreted()
        {
            var eventGraph = WorkflowStartedEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new WorkflowStartedEvent(eventGraph.First()) }));
        }

        [Test]
        public void Timer_fired_event_is_interpreted()
        {
            var eventGraph = TimerFiredEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new TimerFiredEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void Timer_failed_event_is_interpreted()
        {
            var eventGraph = TimerStartFailedEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new TimerStartFailedEvent(eventGraph.First()) }));
        }
        [Test]
        public void Timer_cancellation_failed_event_is_interpreted()
        {
            var eventGraph = TimerCancellationFailedEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new TimerCancellationFailedEvent(eventGraph.First()) }));
        }
        [Test]
        public void Workflow_signaled_event_is_interpreted()
        {
            var eventGraph = WorkflowSignaledEventGraph();
            var events = new WorkflowHistoryEvents(new []{eventGraph});
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new WorkflowSignaledEvent(eventGraph) }));
        }

        [Test]
        public void Workflow_signaled_failed_event_is_interpreted()
        {
            var eventGraph = _builder.WorkflowSignalFailedEvent("cause", "wid", "rid");
            var events = new WorkflowHistoryEvents(new[] { eventGraph });
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new WorkflowSignalFailedEvent(eventGraph) }));
        }
        [Test]
        public void Workflow_cancellation_requested_event_is_interpreted()
        {
            var eventGraph = WorkflowCancellationRequestedEventGraph();
            var events = new WorkflowHistoryEvents(new[] { eventGraph });
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new WorkflowCancellationRequestedEvent(eventGraph) }));
        }
        [Test]
        public void Workflow_completion_failed_event_is_interpreted()
        {
            var eventGraph = _builder.WorkflowCompletionFailureEvent("cause");
            var events = new WorkflowHistoryEvents(new[] { eventGraph });
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new WorkflowCompletionFailedEvent(eventGraph) }));
        }
        [Test]
        public void Workflow_failure_failed_event_is_interpreted()
        {
            var eventGraph = _builder.WorkflowFailureFailedEvent("cause");
            var events = new WorkflowHistoryEvents(new[] { eventGraph });
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new WorkflowFailureFailedEvent(eventGraph) }));
        }

        [Test]
        public void Workflow_restart_failed_event_is_interpreted()
        {
            var eventGraph = _builder.WorkflowRestartFailedEventGraph("cause");
            var events = new WorkflowHistoryEvents(new[] { eventGraph });
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new WorkflowRestartFailedEvent(eventGraph) }));
        }

        [Test]
        public void External_workflow_cancel_request_failed_event_is_interpreted()
        {
            var eventGraph = _builder.ExternalWorkflowCancelRequestFailedEvent(Identity.New("w","v").ScheduleId(),"rid","cause");
            var events = new WorkflowHistoryEvents(eventGraph );
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new ExternalWorkflowCancelRequestFailedEvent(eventGraph.First()) }));
        }

        [Test]
        public void Workflow_cancellation_failed_event_is_interpreted()
        {
            var eventGraph = _builder.WorkflowCancellationFailedEvent("cause");
            var events = new WorkflowHistoryEvents(new[] { eventGraph });
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new WorkflowCancellationFailedEvent(eventGraph) }));
        }

        [Test]
        public void Lambda_function_completed_event_is_interpreted()
        {
            var eventGraph = _builder.LambdaCompletedEventGraph(Identity.Lambda("l").ScheduleId(), "input", "result").ToArray();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new []{new LambdaCompletedEvent(eventGraph.First(), eventGraph)}));
        }

        [Test]
        public void Lambda_function_failed_event_is_interpreted()
        {
            var eventGraph = _builder.LambdaFailedEventGraph(Identity.Lambda("l").ScheduleId(), "input", "reason", "details").ToArray();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new LambdaFailedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void Lambda_function_timedout_event_is_interpreted()
        {
            var eventGraph = _builder.LamdbaTimedoutEventGraph(Identity.Lambda("l").ScheduleId(), "input", "reason").ToArray();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new LambdaTimedoutEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void Lambda_function_scheduling_failed_event_is_interpreted()
        {
            var eventGraph = new []{_builder.LambdaSchedulingFailedEventGraph(Identity.Lambda("l").ScheduleId(), "reason")};
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new LambdaSchedulingFailedEvent(eventGraph.First()) }));
        }

        [Test]
        public void Lambda_function_start_failed_event_is_interpreted()
        {
            var eventGraph =  _builder.LambdaStartFailedEventGraph(Identity.Lambda("l").ScheduleId(), "input", "reason", "details");
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new LambdaStartFailedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void Child_workflow_completed_event_is_interpreted()
        {
            var eventGraph = _builder.ChildWorkflowCompletedGraph(_childWorkflow.ScheduleId(), "rid","i","result").ToArray();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ChildWorkflowStartedEvent(eventGraph.Skip(1).First(), eventGraph),
                new ChildWorkflowCompletedEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void Child_workflow_failed_event_is_interpreted()
        {
            var eventGraph = _builder.ChildWorkflowFailedEventGraph(_childWorkflow.ScheduleId(), "rid", "i", "reason", "details").ToArray();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ChildWorkflowStartedEvent(eventGraph.Skip(1).First(), eventGraph),
                new ChildWorkflowFailedEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void Child_workflow_cancelled_event_is_interpreted()
        {
            var eventGraph = _builder.ChildWorkflowCancelledEventGraph(_childWorkflow.ScheduleId(), "rid", "i", "details").ToArray();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new WorkflowItemEvent[]
            {   new ChildWorkflowStartedEvent(eventGraph.Skip(3).First(), eventGraph),
                new ChildWorkflowCancelledEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void Child_workflow_timedout_event_is_interpreted()
        {
            var eventGraph = _builder.ChildWorkflowTimedoutEventGraph(_childWorkflow.ScheduleId(), "rid", "i", "details").ToArray();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ChildWorkflowStartedEvent(eventGraph.Skip(1).First(), eventGraph),
                new ChildWorkflowTimedoutEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void Child_workflow_terminated_event_is_interpreted()
        {
            var eventGraph = _builder.ChildWorkflowTerminatedEventGraph(_childWorkflow.ScheduleId(), "rid", "i").ToArray();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ChildWorkflowStartedEvent(eventGraph.Skip(1).First(), eventGraph),
                new  ChildWorkflowTerminatedEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void Child_workflow_start_failed_event_is_interpreted()
        {
            var eventGraph = _builder.ChildWorkflowStartFailedEventGraph(_childWorkflow.ScheduleId(), "rid", "i");
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.EqualTo(new[] { new ChildWorkflowStartFailedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void Non_interpretable_events_are_filtered_out()
        {
            var eventGraph = NotInterpretingEventGraph();
            var events = new WorkflowHistoryEvents(eventGraph);
            var newEvents = events.NewEvents();

            Assert.That(newEvents, Is.Empty);
        }


        [Test]
        public void Should_be_active_when_activity_is_just_started()
        {
            var activityStartedEventGraph = _builder.ActivityStartedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id");
            var workflowHistoryEvents = new WorkflowHistoryEvents(activityStartedEventGraph);

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_be_active_when_activity_is_just_scheduled()
        {
            var activityScheduledEventGraph = _builder.ActivityScheduledGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId());
            var workflowHistoryEvents = new WorkflowHistoryEvents(activityScheduledEventGraph);

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_be_active_when_activity_cancellation_is_in_progress()
        {
            var activityCancelRequestedGraph = _builder.ActivityCancelRequestedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id");
            var workflowHistoryEvents = new WorkflowHistoryEvents(activityCancelRequestedGraph);

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }


        [Test]
        public void Should_not_be_active_when_activity_is_cancelled()
        {
            var cancelledGraph = _builder.ActivityCancelledGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id", "details");
            var workflowHistoryEvents = new WorkflowHistoryEvents(cancelledGraph);

            Assert.IsFalse(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_not_be_active_when_activity_is_completed()
        {
            var activityCompletedEventGraph =_builder.ActivityCompletedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id", "res");
            var workflowHistoryEvents = new WorkflowHistoryEvents(activityCompletedEventGraph);

            Assert.IsFalse(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_be_active_when_activity_is_just_started_after_failure()
        {
            var eventGraph = _builder.ActivityStartedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id")
                                            .Concat(_builder.ActivityFailedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id", "res","detail"));
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_be_active_when_activity_is_started_but_its_cancel_request_failed()
        {
            var eventGraph = _builder.ActivityCancellationFailedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "reason")
                .Concat(_builder.ActivityFailedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id", "res", "detail"));
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_not_be_active_when_child_workflow_is_cancelled()
        {
            var eventGraph = _builder.ChildWorkflowCancelledEventGraph(Identity.New(WorkflowName, WorkflowVersion).ScheduleId(),
                "rid", "input", "detail");
                
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);

            Assert.IsFalse(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_be_active_when_child_workflow_is_started_and_its_cancellation_is_requested()
        {
            var eventGraph = _builder.ChildWorkflowCancellationRequestedEventGraph(Identity.New(WorkflowName, WorkflowVersion).ScheduleId(),
                "rid", "input");

            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_be_active_when_child_workflow_is_started_and_its_cancel_request_is_failed()
        {
            var startedGraph =
                _builder.ChildWorkflowStartedEventGraph(Identity.New(WorkflowName, WorkflowVersion).ScheduleId(), "rid", "input");
            var cancelFailedGraph = _builder.ExternalWorkflowCancelRequestFailedEvent(Identity.New(WorkflowName, WorkflowVersion).ScheduleId(),
                "rid", "input");

            var workflowHistoryEvents = new WorkflowHistoryEvents(cancelFailedGraph.Concat(startedGraph));

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_not_be_active_when_child_workflow_is_completed()
        {
            var eventGraph = _builder.ChildWorkflowCompletedGraph(Identity.New(WorkflowName, WorkflowVersion).ScheduleId(),
                "rid", "input", "detail");

            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);

            Assert.IsFalse(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_be_active_when_lambda_is_started()
        {
            var eventGraph = _builder.LambdaStartedEventGraph(Identity.Lambda(LambdaName).ScheduleId(), "input");

            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_be_active_when_timer_is_started()
        {
            var timerStartedEventGraph = _builder.TimerStartedGraph(Identity.Timer("id").ScheduleId(), TimeSpan.FromSeconds(2));
            var workflowHistoryEvents = new WorkflowHistoryEvents(timerStartedEventGraph);

            Assert.IsTrue(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Should_not_be_active_when_timer_is_fired()
        {
            var timerStartedEventGraph = _builder.TimerFiredGraph(Identity.Timer("id").ScheduleId(), TimeSpan.FromSeconds(2));
            var workflowHistoryEvents = new WorkflowHistoryEvents(timerStartedEventGraph);

            Assert.IsFalse(workflowHistoryEvents.HasActiveEvent());
        }

        [Test]
        public void Can_return_all_marker_recorded_events()
        {
            var markerRecordedEventGraph = new[]
            {
                _builder.MarkerRecordedEvent("name1", "detail1"),
                _builder.MarkerRecordedEvent("name2", "detail2")
            };
            var workflowHistoryEvents = new WorkflowHistoryEvents(markerRecordedEventGraph);
            var markerRecordedEvents = workflowHistoryEvents.AllMarkerRecordedEvents();

            Assert.That(markerRecordedEvents,Is.EqualTo(new []{new MarkerRecordedEvent(markerRecordedEventGraph.First()),
                new MarkerRecordedEvent(markerRecordedEventGraph.Last())}));
        }

        [Test]
        public void Can_return_all_signal_events()
        {
            var signalEventsGraph = new[]
            {
                _builder.WorkflowSignaledEvent("name1", "input1"),
                _builder.WorkflowSignaledEvent("name1", "input1","runid","wid")
            };
            var workflowHistoryEvents = new WorkflowHistoryEvents(signalEventsGraph);
            var allSignalEvents = workflowHistoryEvents.AllSignalEvents();

            Assert.That(allSignalEvents,Is.EqualTo(new []{ new WorkflowSignaledEvent(signalEventsGraph.First()),new WorkflowSignaledEvent(signalEventsGraph.Last())}));
        }

        [Test]
        public void Can_return_all_cancellation_request()
        {
            var cancellationEventGraph = new[]
            {
                _builder.WorkflowCancellationRequestedEvent("cause"),
                _builder.WorkflowCancellationRequestedEvent("cause2","runid","wid")
            };
            var workflowHistoryEvents = new WorkflowHistoryEvents(cancellationEventGraph);
            var allWorkflowCancellationRequestedEvents = workflowHistoryEvents.AllWorkflowCancellationRequestedEvents();

            Assert.That(allWorkflowCancellationRequestedEvents, Is.EqualTo(new[] { new WorkflowCancellationRequestedEvent(cancellationEventGraph.First()), new WorkflowCancellationRequestedEvent(cancellationEventGraph.Last()) }));
        }

        [Test]
        public void Latest_event_id()
        {
            var events = _builder.TimerFiredGraph(Identity.Timer("id").ScheduleId(), TimeSpan.FromSeconds(2));
            var workflowHistoryEvents = new WorkflowHistoryEvents(events);

            Assert.That(workflowHistoryEvents.LatestEventId, Is.EqualTo(events.First().EventId));
        }

        private HistoryEvent[] ActivityCompletedEventGraph()
        {
            return  _builder.ActivityCompletedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id", "result").ToArray();
        }
        private HistoryEvent [] ActivityFailedEventGraph()
        {
            return _builder.ActivityFailedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id", "reason","detail").ToArray();
        }
        private HistoryEvent [] ActivityTimedoutEventGraph()
        {
            return _builder.ActivityTimedoutGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id", "reason", "detail").ToArray();
        }
        private HistoryEvent [] ActivityCancelledEventGraph()
        {
            return _builder.ActivityCancelledGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id", "detail").ToArray();
        }
        private HistoryEvent [] ActivityCancellationFailedEventGraph()
        {
            return _builder.ActivityCancellationFailedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "detail").ToArray();
        }
        private HistoryEvent [] WorkflowStartedEventGraph()
        {
            return _builder.WorkflowStartedGraph().ToArray();
        }
        private HistoryEvent [] TimerFiredEventGraph()
        {
            return _builder.TimerFiredGraph(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromSeconds(4)).ToArray();
        }
        private HistoryEvent [] TimerStartFailedEventGraph()
        {
            return _builder.TimerStartFailedGraph(Identity.Timer(TimerName).ScheduleId(), "cause").ToArray();
        }
        private HistoryEvent [] TimerCancelledEventGraph()
        {
            return _builder.TimerCancelledGraph(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromSeconds(4)).ToArray();
        }
        private HistoryEvent [] TimerCancellationFailedEventGraph()
        {
            return _builder.TimerCancellationFailedGraph(Identity.Timer(TimerName).ScheduleId(), "cause").ToArray();
        }
        private HistoryEvent WorkflowSignaledEventGraph()
        {
            return _builder.WorkflowSignaledEvent("name","input");
        }
        private HistoryEvent WorkflowCancellationRequestedEventGraph()
        {
            return _builder.WorkflowCancellationRequestedEvent("cause");
        }
        private HistoryEvent [] NotInterpretingEventGraph()
        {
            var nonInterpretEvent = new[] {new HistoryEvent() {EventType = EventType.DecisionTaskCompleted}};
            var activityStarted = _builder.ActivityStartedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id");
            var activityScheduled = _builder.ActivityScheduledGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId());
            var timerStarted = _builder.TimerStartedGraph(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromSeconds(1));
            var childWorfklowStarted = _builder.ChildWorkflowStartedEventGraph(_childWorkflow.ScheduleId(), "rid", "input");
            var waitForSignalEvents =new []{ _builder.WaitForSignalEvent(Identity.Timer(TimerName).ScheduleId(), 1,new[] {"Event1"}, SignalWaitType.Any)};
            return timerStarted.Concat(activityScheduled)
                .Concat(activityStarted).Concat(nonInterpretEvent).Concat(childWorfklowStarted).Concat(waitForSignalEvents).ToArray();
        }
    }
}