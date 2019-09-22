// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using ChildPolicy = Guflow.Decider.ChildPolicy;

namespace Guflow.Tests.Decider
{
    /// <summary>
    /// TODO: Refactor this class in to different builder classes for Activity, Timer, Lambda, ChildWorkfow and Workflow
    /// </summary>
    internal class EventGraphBuilder
    {
        private long _currentEventId = 0;
        public IEnumerable<HistoryEvent> ActivityCompletedGraph(ScheduleId activityIdentity, string workerIdentity, string result, string input = "")
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CompletedIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCompleted,
                EventId = eventIds.EventId(EventIds.Completion),
                ActivityTaskCompletedEventAttributes = new ActivityTaskCompletedEventAttributes()
                {
                    Result = result,
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.EventId(EventIds.Started),
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = workerIdentity,
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.EventId(EventIds.Scheduled),
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity,
                    Input = input
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityFailedGraph(ScheduleId activityIdentity, string identity, string reason, string detail)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.FailedIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskFailed,
                EventId = eventIds.EventId(EventIds.Failed),
                ActivityTaskFailedEventAttributes = new ActivityTaskFailedEventAttributes()
                {
                    Details = detail,
                    Reason = reason,
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.EventId(EventIds.Started),
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.EventId(EventIds.Scheduled),
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityTimedoutGraph(ScheduleId activityIdentity, string identity, string timeoutType, string detail)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimedoutIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskTimedOut,
                EventId = eventIds.EventId(EventIds.Timedout),
                ActivityTaskTimedOutEventAttributes = new ActivityTaskTimedOutEventAttributes()
                {
                    Details = detail,
                    TimeoutType = new ActivityTaskTimeoutType(timeoutType),
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.EventId(EventIds.Started),
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.EventId(EventIds.Scheduled),
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityCancelledGraph(ScheduleId activityIdentity, string identity, string detail)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CancelledIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCanceled,
                EventId = eventIds.EventId(EventIds.Cancelled),
                ActivityTaskCanceledEventAttributes = new ActivityTaskCanceledEventAttributes()
                {
                    Details = detail,
                    LatestCancelRequestedEventId = eventIds.EventId(EventIds.CancelRequested),
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCancelRequested,
                EventId = eventIds.EventId(EventIds.CancelRequested),
                ActivityTaskCancelRequestedEventAttributes = new ActivityTaskCancelRequestedEventAttributes()
                {
                    ActivityId = activityIdentity
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.EventId(EventIds.Started),
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.EventId(EventIds.Scheduled),
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerFiredGraph(ScheduleId timerId, TimeSpan startToFireTimeout, bool isARescheduleTimer = false)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimerFiredIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerFired,
                EventId = eventIds.EventId(EventIds.TimerFired),
                TimerFiredEventAttributes = new TimerFiredEventAttributes()
                {
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    TimerId = timerId
                },
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.EventId(EventIds.Started),
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId,
                    StartToFireTimeout = ((long)startToFireTimeout.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, IsARescheduleTimer = isARescheduleTimer }).ToJson()
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerFiredGraph(ScheduleId timerId, TimeSpan startToFireTimeout, TimerType timerType)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimerFiredIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerFired,
                EventId = eventIds.EventId(EventIds.TimerFired),
                TimerFiredEventAttributes = new TimerFiredEventAttributes()
                {
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    TimerId = timerId
                },
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.EventId(EventIds.Started),
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId,
                    StartToFireTimeout = ((long)startToFireTimeout.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, TimerType = timerType }).ToJson()
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerCancelledGraph(ScheduleId timerId, TimeSpan startToFireTimeout, bool isARescheduleTimer = false)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimerCancelledIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerCanceled,
                EventId = eventIds.EventId(EventIds.Cancelled),
                TimerCanceledEventAttributes = new TimerCanceledEventAttributes()
                {
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    TimerId = timerId,
                },
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.EventId(EventIds.Started),
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId,
                    StartToFireTimeout = ((long)startToFireTimeout.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, IsARescheduleTimer = isARescheduleTimer }).ToJson()
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerStartFailedGraph(ScheduleId timerId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimerStartFailedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.StartTimerFailed,
                EventId = eventIds.EventId(EventIds.Failed),
                StartTimerFailedEventAttributes = new StartTimerFailedEventAttributes()
                {
                    TimerId = timerId,
                    Cause = cause
                },
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> WorkflowStartedGraph(string input = "workflowinput")
        {
            return new[] { WorkflowStartedEvent(input) };
        }

        public HistoryEvent WorkflowStartedEvent(object input = null)
        {
            return new HistoryEvent()
            {
                EventType = EventType.WorkflowExecutionStarted,
                WorkflowExecutionStartedEventAttributes = new WorkflowExecutionStartedEventAttributes()
                {
                    ChildPolicy = ChildPolicy.Terminate,
                    ContinuedExecutionRunId = "continue run id",
                    ExecutionStartToCloseTimeout = "100",
                    Input = input.ToAwsString(),
                    LambdaRole = "some role",
                    ParentInitiatedEventId = 10,
                    ParentWorkflowExecution = new WorkflowExecution() { RunId = "parent runid", WorkflowId = "parent workflow id" },
                    TagList = new List<string>() { "First", "Second" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "task name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "30",
                }
            };
        }
        public IEnumerable<HistoryEvent> ActivityCancellationFailedGraph(ScheduleId activityId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CompletedIds(ref _currentEventId);


            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.RequestCancelActivityTaskFailed,
                EventId = eventIds.EventId(EventIds.Completion),
                RequestCancelActivityTaskFailedEventAttributes = new RequestCancelActivityTaskFailedEventAttributes()
                {
                    ActivityId = activityId,
                    Cause = cause
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.EventId(EventIds.Started),
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = "someid",
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.EventId(EventIds.Scheduled),
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityId.Name, Version = activityId.Version },
                    ActivityId = activityId,
                    Control = (new ScheduleData() { PN = activityId.PositionalName }).ToJson(),
                }
            });
            return historyEvents;
        }
        public IEnumerable<HistoryEvent> TimerCancellationFailedGraph(ScheduleId timerId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimerCancellationFailedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.CancelTimerFailed,
                EventId = eventIds.EventId(EventIds.Failed),
                CancelTimerFailedEventAttributes = new CancelTimerFailedEventAttributes()
                {
                    TimerId = timerId,
                    Cause = cause
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerStartedGraph(ScheduleId identity, TimeSpan fireAfter, bool isARescheduleTimer = false)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimerStartedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.EventId(EventIds.Started),
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = identity,
                    StartToFireTimeout = ((long)fireAfter.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = identity.Name, IsARescheduleTimer = isARescheduleTimer }).ToJson()
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivitySchedulingFailedGraph(ScheduleId activityIdentity, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.SchedulingFailedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ScheduleActivityTaskFailed,
                EventId = eventIds.EventId(EventIds.Failed),
                ScheduleActivityTaskFailedEventAttributes = new ScheduleActivityTaskFailedEventAttributes()
                {
                    ActivityId = activityIdentity,
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Cause = cause
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityScheduledGraph(ScheduleId activityIdentity)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.ScheduledIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.EventId(EventIds.Scheduled),
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityStartedGraph(ScheduleId scheduleId, string identity)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.StartedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.EventId(EventIds.Started),
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.EventId(EventIds.Scheduled),
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = scheduleId.Name, Version = scheduleId.Version },
                    Control = (new ScheduleData() { PN = scheduleId.PositionalName }).ToJson(),
                    ActivityId = scheduleId
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityCancelRequestedGraph(ScheduleId activityIdentity, string identity)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.ActivityCancelRequestedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCancelRequested,
                EventId = eventIds.EventId(EventIds.CancelRequested),
                ActivityTaskCancelRequestedEventAttributes = new ActivityTaskCancelRequestedEventAttributes()
                {
                    ActivityId = activityIdentity,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.EventId(EventIds.Started),
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.EventId(EventIds.Scheduled),
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity
                }
            });
            return historyEvents;
        }


        public HistoryEvent WorkflowSignaledEvent(string signalName, string input, string externalWorkflowRunId, string externalWorkflowId)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);

            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.WorkflowExecutionSignaled,
                WorkflowExecutionSignaledEventAttributes = new WorkflowExecutionSignaledEventAttributes()
                {
                    SignalName = signalName,
                    Input = input,
                    ExternalWorkflowExecution = new WorkflowExecution() { RunId = externalWorkflowRunId, WorkflowId = externalWorkflowId }
                }
            };
        }
        public HistoryEvent WorkflowSignaledEvent(string signalName, string input, DateTime? completedTime=null)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.WorkflowExecutionSignaled,
                EventTimestamp = completedTime??DateTime.UtcNow,
                WorkflowExecutionSignaledEventAttributes = new WorkflowExecutionSignaledEventAttributes()
                {
                    SignalName = signalName,
                    Input = input,
                }
            };
        }
        public HistoryEvent WorkflowCancellationRequestedEvent(string cause, string externalWorkflowRunid, string externalWorkflowId)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.WorkflowExecutionCancelRequested,
                WorkflowExecutionCancelRequestedEventAttributes = new WorkflowExecutionCancelRequestedEventAttributes()
                {
                    Cause = cause,
                    ExternalWorkflowExecution = new WorkflowExecution()
                    {
                        RunId = externalWorkflowRunid,
                        WorkflowId = externalWorkflowId
                    }
                }
            };
        }
        public HistoryEvent WorkflowCancellationRequestedEvent(string cause)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.WorkflowExecutionCancelRequested,
                WorkflowExecutionCancelRequestedEventAttributes = new WorkflowExecutionCancelRequestedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }

        public HistoryEvent MarkerRecordedEvent(string markerName, string detail1)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.MarkerRecorded,
                MarkerRecordedEventAttributes = new MarkerRecordedEventAttributes()
                {
                    MarkerName = markerName,
                    Details = detail1
                }
            };
        }

        public HistoryEvent WaitForSignalEvent(ScheduleId id, long eventId, string[] eventNames,
            SignalWaitType waitType, SignalNextAction nextAction = SignalNextAction.Continue)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            var details = new WaitForSignalData()
            {
                ScheduleId = id,
                TriggerEventId = eventId,
                WaitType = waitType,
                SignalNames = eventNames,
                NextAction = nextAction
            };
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.MarkerRecorded,
                MarkerRecordedEventAttributes = new MarkerRecordedEventAttributes()
                {
                    MarkerName = InternalMarkerNames.WorkflowItemWaitForSignals,
                    Details = details.ToJson()
                }
            };
        }

        public HistoryEvent WorkflowItemSignalledEvent(ScheduleId id, long eventId, string eventName)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            var details = new WorkflowItemSignalledData()
            {
                ScheduleId = id,
                TriggerEventId = eventId,
                SignalName = eventName,
            };
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.MarkerRecorded,
                MarkerRecordedEventAttributes = new MarkerRecordedEventAttributes()
                {
                    MarkerName = InternalMarkerNames.WorkflowItemSignalled,
                    Details = details.ToJson()
                }
            };
        }

        public HistoryEvent RecordMarkerFailedEvent(string markerName, string cause)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.RecordMarkerFailed,
                RecordMarkerFailedEventAttributes = new RecordMarkerFailedEventAttributes()
                {
                    MarkerName = markerName,
                    Cause = cause
                }
            };
        }

        public HistoryEvent WorkflowSignalFailedEvent(string cause, string workflowId, string runId)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.SignalExternalWorkflowExecutionFailed,
                SignalExternalWorkflowExecutionFailedEventAttributes = new SignalExternalWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                    WorkflowId = workflowId,
                    RunId = runId
                }
            };
        }

        public HistoryEvent WorkflowCompletionFailureEvent(string cause)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.CompleteWorkflowExecutionFailed,
                CompleteWorkflowExecutionFailedEventAttributes = new CompleteWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }

        public HistoryEvent WorkflowFailureFailedEvent(string cause)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.FailWorkflowExecutionFailed,
                FailWorkflowExecutionFailedEventAttributes = new FailWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }
        public IEnumerable<HistoryEvent> ExternalWorkflowCancelRequestFailedEvent(ScheduleId id, string runid, string cause)
        {
            var events = new List<HistoryEvent>();
            var eventIds = EventIds.WorkflowCancelRequestFailedIds(ref _currentEventId);
            events.Add(new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.CancelRequestFailed),
                EventType = EventType.RequestCancelExternalWorkflowExecutionFailed,
                RequestCancelExternalWorkflowExecutionFailedEventAttributes = new RequestCancelExternalWorkflowExecutionFailedEventAttributes()
                {
                    WorkflowId = id,
                    RunId = runid,
                    Cause = cause,
                    InitiatedEventId = eventIds.EventId(EventIds.CancelInitiated)
                }
            });

            events.Add(new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.CancelInitiated),
                EventType = EventType.RequestCancelExternalWorkflowExecutionInitiated,
                RequestCancelExternalWorkflowExecutionInitiatedEventAttributes = new RequestCancelExternalWorkflowExecutionInitiatedEventAttributes()
                {
                    WorkflowId = id,
                    RunId = runid,
                }
            });

            return events;
        }
        public HistoryEvent WorkflowCancellationFailedEvent(string cause)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.CancelWorkflowExecutionFailed,
                CancelWorkflowExecutionFailedEventAttributes = new CancelWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }

        public IEnumerable<HistoryEvent> LambdaCompletedEventGraph(ScheduleId identity, object input, object result, TimeSpan? startToClose = null, DateTime? completedStamp=null)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CompletedIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Completion),
                EventType = EventType.LambdaFunctionCompleted,
                EventTimestamp = completedStamp ?? DateTime.UtcNow,
                LambdaFunctionCompletedEventAttributes = new LambdaFunctionCompletedEventAttributes()
                {
                    Result = result.ToAwsString(),
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.LambdaFunctionStarted,

                LambdaFunctionStartedEventAttributes = new LambdaFunctionStartedEventAttributes()
                {
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.LambdaFunctionScheduled,
                LambdaFunctionScheduledEventAttributes = new LambdaFunctionScheduledEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    Id = identity,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = startToClose.Seconds()
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> LambdaFailedEventGraph(ScheduleId identity, object input, string reason, string details, TimeSpan? timeout = null)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.FailedIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Failed),
                EventType = EventType.LambdaFunctionFailed,
                LambdaFunctionFailedEventAttributes = new LambdaFunctionFailedEventAttributes()
                {
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    Reason = reason,
                    Details = details,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.LambdaFunctionStarted,

                LambdaFunctionStartedEventAttributes = new LambdaFunctionStartedEventAttributes()
                {
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.LambdaFunctionScheduled,
                LambdaFunctionScheduledEventAttributes = new LambdaFunctionScheduledEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    Id = identity,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> LamdbaTimedoutEventGraph(ScheduleId identity, object input, string timedoutType, TimeSpan? timeout = null)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimedoutIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Timedout),
                EventType = EventType.LambdaFunctionTimedOut,
                LambdaFunctionTimedOutEventAttributes = new LambdaFunctionTimedOutEventAttributes
                {
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    TimeoutType = timedoutType,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.LambdaFunctionStarted,

                LambdaFunctionStartedEventAttributes = new LambdaFunctionStartedEventAttributes()
                {
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.LambdaFunctionScheduled,
                LambdaFunctionScheduledEventAttributes = new LambdaFunctionScheduledEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    Id = identity,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            });

            return historyEvents;
        }

        public HistoryEvent LambdaSchedulingFailedEventGraph(ScheduleId identity, string reason)
        {
            var eventIds = EventIds.SchedulingFailedIds(ref _currentEventId);
            return new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Failed),
                EventType = EventType.ScheduleLambdaFunctionFailed,
                ScheduleLambdaFunctionFailedEventAttributes = new ScheduleLambdaFunctionFailedEventAttributes
                {
                    Id = identity,
                    Name = identity.Name,
                    Cause = reason
                }
            };
        }

        public IEnumerable<HistoryEvent> LambdaStartFailedEventGraph(ScheduleId identity, string input, string cause, string message, TimeSpan? timeout = null)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.LambdaStartFailedIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Failed),
                EventType = EventType.StartLambdaFunctionFailed,
                StartLambdaFunctionFailedEventAttributes = new StartLambdaFunctionFailedEventAttributes
                {
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled),
                    Cause = cause,
                    Message = message
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.LambdaFunctionScheduled,
                LambdaFunctionScheduledEventAttributes = new LambdaFunctionScheduledEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    Id = identity,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            });

            return historyEvents;
        }

        public HistoryEvent LambdaScheduledEventGraph(ScheduleId identity, object input, TimeSpan? timeout = null)
        {
            var eventIds = EventIds.ScheduledIds(ref _currentEventId);
            return new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.LambdaFunctionScheduled,
                LambdaFunctionScheduledEventAttributes = new LambdaFunctionScheduledEventAttributes
                {
                    Id = identity,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            };
        }

        public IEnumerable<HistoryEvent> LambdaStartedEventGraph(ScheduleId identity, object input, TimeSpan? timeout = null)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.StartedIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.LambdaFunctionStarted,
                LambdaFunctionStartedEventAttributes = new LambdaFunctionStartedEventAttributes
                {
                    ScheduledEventId = eventIds.EventId(EventIds.Scheduled),
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.LambdaFunctionScheduled,
                LambdaFunctionScheduledEventAttributes = new LambdaFunctionScheduledEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    Id = identity,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            });

            return historyEvents;
        }


        public IEnumerable<HistoryEvent> ChildWorkflowCompletedGraph(ScheduleId identity, string runId, object input, object result)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CompletedIds(ref _currentEventId);
            var workflowExecution = new WorkflowExecution() { RunId = runId, WorkflowId = identity };
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Completion),
                EventType = EventType.ChildWorkflowExecutionCompleted,
                ChildWorkflowExecutionCompletedEventAttributes = new ChildWorkflowExecutionCompletedEventAttributes()
                {
                    Result = result.ToAwsString(),
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.ChildWorkflowExecutionStarted,

                ChildWorkflowExecutionStartedEventAttributes = new ChildWorkflowExecutionStartedEventAttributes()
                {
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = input.ToAwsString(),
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }


        public IEnumerable<HistoryEvent> ChildWorkflowFailedEventGraph(ScheduleId identity, string runId, object input, string reason, object details)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.FailedIds(ref _currentEventId);
            var workflowExecution = new WorkflowExecution() { RunId = runId, WorkflowId = identity };
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Failed),
                EventType = EventType.ChildWorkflowExecutionFailed,
                ChildWorkflowExecutionFailedEventAttributes = new ChildWorkflowExecutionFailedEventAttributes()
                {
                    Reason = reason,
                    Details = details.ToAwsString(),
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.ChildWorkflowExecutionStarted,

                ChildWorkflowExecutionStartedEventAttributes = new ChildWorkflowExecutionStartedEventAttributes()
                {
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = input.ToAwsString(),
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ChildWorkflowCancelledEventGraph(ScheduleId identity, string runId, object input, object details)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.ChildWorkflowCancelledIds(ref _currentEventId);
            var workflowExecution = new WorkflowExecution() { RunId = runId, WorkflowId = identity };
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Cancelled),
                EventType = EventType.ChildWorkflowExecutionCanceled,
                ChildWorkflowExecutionCanceledEventAttributes = new ChildWorkflowExecutionCanceledEventAttributes()
                {
                    Details = details.ToAwsString(),
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.CancelRequested),
                EventType = EventType.ExternalWorkflowExecutionCancelRequested,
                ExternalWorkflowExecutionCancelRequestedEventAttributes = new ExternalWorkflowExecutionCancelRequestedEventAttributes()
                {
                    InitiatedEventId = eventIds.EventId(EventIds.CancelInitiated),
                    WorkflowExecution = workflowExecution,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.CancelInitiated),
                EventType = EventType.RequestCancelExternalWorkflowExecutionInitiated,
                RequestCancelExternalWorkflowExecutionInitiatedEventAttributes = new RequestCancelExternalWorkflowExecutionInitiatedEventAttributes()
                {
                    WorkflowId = workflowExecution.WorkflowId,
                    RunId = workflowExecution.RunId
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.ChildWorkflowExecutionStarted,

                ChildWorkflowExecutionStartedEventAttributes = new ChildWorkflowExecutionStartedEventAttributes()
                {
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = input.ToAwsString(),
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ChildWorkflowStartedEventGraph(ScheduleId identity, string runId, object input)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.StartedIds(ref _currentEventId);
            var workflowExecution = new WorkflowExecution() { RunId = runId, WorkflowId = identity };
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.ChildWorkflowExecutionStarted,

                ChildWorkflowExecutionStartedEventAttributes = new ChildWorkflowExecutionStartedEventAttributes()
                {
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = input.ToAwsString(),
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }


        public IEnumerable<HistoryEvent> ChildWorkflowTerminatedEventGraph(ScheduleId identity, string runId, object input)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CompletedIds(ref _currentEventId);
            var workflowExecution = new WorkflowExecution() { RunId = runId, WorkflowId = identity };
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Completion),
                EventType = EventType.ChildWorkflowExecutionTerminated,
                ChildWorkflowExecutionTerminatedEventAttributes = new ChildWorkflowExecutionTerminatedEventAttributes()
                {
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.ChildWorkflowExecutionStarted,

                ChildWorkflowExecutionStartedEventAttributes = new ChildWorkflowExecutionStartedEventAttributes()
                {
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = input.ToAwsString(),
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ChildWorkflowTimedoutEventGraph(ScheduleId identity, string runId, object input, string timedoutType)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimedoutIds(ref _currentEventId);
            var workflowExecution = new WorkflowExecution() { RunId = runId, WorkflowId = identity };
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Timedout),
                EventType = EventType.ChildWorkflowExecutionTimedOut,
                ChildWorkflowExecutionTimedOutEventAttributes = new ChildWorkflowExecutionTimedOutEventAttributes()
                {
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled),
                    StartedEventId = eventIds.EventId(EventIds.Started),
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    TimeoutType = timedoutType
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.ChildWorkflowExecutionStarted,

                ChildWorkflowExecutionStartedEventAttributes = new ChildWorkflowExecutionStartedEventAttributes()
                {
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = input.ToAwsString(),
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ChildWorkflowStartFailedEventGraph(ScheduleId identity, object input, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.ChildWorkflowStartFailed(ref _currentEventId);
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.StartFailed),
                EventType = EventType.StartChildWorkflowExecutionFailed,
                StartChildWorkflowExecutionFailedEventAttributes = new StartChildWorkflowExecutionFailedEventAttributes()
                {
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled),
                    WorkflowType = workflowType,
                    Cause = cause
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = input.ToAwsString(),
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ChildWorkflowCancellationRequestedEventGraph(ScheduleId identity, string runId, string input)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.WorkflowCancelRequestedIds(ref _currentEventId);
            var workflowExecution = new WorkflowExecution() { RunId = runId, WorkflowId = identity };
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.CancelRequested),
                EventType = EventType.ExternalWorkflowExecutionCancelRequested,
                ExternalWorkflowExecutionCancelRequestedEventAttributes = new ExternalWorkflowExecutionCancelRequestedEventAttributes()
                {
                    InitiatedEventId = eventIds.EventId(EventIds.CancelInitiated),
                    WorkflowExecution = workflowExecution
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.CancelInitiated),
                EventType = EventType.RequestCancelExternalWorkflowExecutionInitiated,
                RequestCancelExternalWorkflowExecutionInitiatedEventAttributes = new RequestCancelExternalWorkflowExecutionInitiatedEventAttributes()
                {
                    RunId = workflowExecution.RunId,
                    WorkflowId = workflowExecution.WorkflowId,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.ChildWorkflowExecutionStarted,

                ChildWorkflowExecutionStartedEventAttributes = new ChildWorkflowExecutionStartedEventAttributes()
                {
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled),
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = input.ToAwsString(),
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ChildWorkflowCancelRequestFailedEventGraph(ScheduleId identity, string runId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.ChildWorkflowCancelRequestFailedIds(ref _currentEventId);
            var workflowExecution = new WorkflowExecution() { RunId = runId, WorkflowId = identity };
            var workflowType = new WorkflowType() { Name = identity.Name, Version = identity.Version };

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.CancelRequestFailed),
                EventType = EventType.RequestCancelExternalWorkflowExecutionFailed,
                RequestCancelExternalWorkflowExecutionFailedEventAttributes = new RequestCancelExternalWorkflowExecutionFailedEventAttributes()
                {
                    InitiatedEventId = eventIds.EventId(EventIds.CancelInitiated),
                    WorkflowId = workflowExecution.WorkflowId,
                    RunId = workflowExecution.RunId,
                    Cause = cause,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.CancelInitiated),
                EventType = EventType.RequestCancelExternalWorkflowExecutionInitiated,
                RequestCancelExternalWorkflowExecutionInitiatedEventAttributes = new RequestCancelExternalWorkflowExecutionInitiatedEventAttributes()
                {
                    RunId = workflowExecution.RunId,
                    WorkflowId = workflowExecution.WorkflowId,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Started),
                EventType = EventType.ChildWorkflowExecutionStarted,

                ChildWorkflowExecutionStartedEventAttributes = new ChildWorkflowExecutionStartedEventAttributes()
                {
                    WorkflowExecution = workflowExecution,
                    WorkflowType = workflowType,
                    InitiatedEventId = eventIds.EventId(EventIds.Scheduled)
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.StartChildWorkflowExecutionInitiated,
                StartChildWorkflowExecutionInitiatedEventAttributes = new StartChildWorkflowExecutionInitiatedEventAttributes()
                {
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    WorkflowId = identity,
                    WorkflowType = workflowType,
                    Input = "input",
                    LambdaRole = "lambda_role",
                    ExecutionStartToCloseTimeout = "3",
                    TagList = new List<string>() { "tag1" },
                    TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = "name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "4"
                }
            });

            return historyEvents;
        }

        public HistoryEvent WorkflowRestartFailedEventGraph(string cause)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.ContinueAsNewWorkflowExecutionFailed,
                ContinueAsNewWorkflowExecutionFailedEventAttributes = new ContinueAsNewWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }


        public HistoryEvent DecisionStartedEvent()
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.DecisionTaskStarted,
                DecisionTaskStartedEventAttributes = new DecisionTaskStartedEventAttributes()
                {
                    Identity = "id"
                }
            };
        }

        private class EventIds
        {
            public const string Completion = "Completion";
            public const string Started = "Started";
            public const string Scheduled = "Scheduled";
            public const string CancelRequested = "CancelRequested";
            public const string Cancelled = "Cancelled";
            public const string TimerFired = "TimerFired";
            public const string Failed = "Failed";
            public const string Timedout = "Timedout";
            public const string Generic = "Generic";
            public const string StartFailed = "StartFailed";
            public const string CancelInitiated = "CancelInitiated";
            public const string CancelRequestFailed = "CancelRequestFailed";
            private readonly Dictionary<string, long> _ids;

            private EventIds(Dictionary<string, long> ids)
            {
                _ids = ids;
            }

            public long EventId(string eventType)
            {
                return _ids[eventType];
            }
            public static EventIds CompletedIds(ref long eventId)
            {
                const long totalEvents = 3;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Completion, eventId},
                    {Started, eventId - 1},
                    {Scheduled, eventId - 2}
                };
                return new EventIds(ids);
            }

            public static EventIds FailedIds(ref long eventId)
            {
                const long totalEvents = 3;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Failed, eventId},
                    {Started, eventId - 1},
                    {Scheduled, eventId - 2}
                };
                return new EventIds(ids);
            }

            public static EventIds TimedoutIds(ref long eventId)
            {
                const long totalEvents = 3;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Timedout, eventId},
                    {Started, eventId - 1},
                    {Scheduled, eventId - 2}
                };
                return new EventIds(ids);
            }
            public static EventIds CancelledIds(ref long eventId)
            {
                const long totalEvents = 4;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Cancelled, eventId},
                    {CancelRequested, eventId-1},
                    {Started, eventId - 2},
                    {Scheduled, eventId - 3}
                };
                return new EventIds(ids);
            }
            public static EventIds TimerFiredIds(ref long eventId)
            {
                const long totalEvents = 2;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {TimerFired, eventId},
                    {Started, eventId - 1},
                };
                return new EventIds(ids);
            }

            public static EventIds TimerCancelledIds(ref long eventId)
            {
                const long totalEvents = 2;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Cancelled, eventId},
                    {Started, eventId - 1},
                };
                return new EventIds(ids);
            }

            public static EventIds TimerStartFailedIds(ref long eventId)
            {
                const long totalEvents = 1;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Failed, eventId},
                };
                return new EventIds(ids);
            }

            public static EventIds LambdaStartFailedIds(ref long eventId)
            {
                const long totalEvents = 2;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Failed, eventId},
                    {Scheduled, eventId-1},
                };
                return new EventIds(ids);
            }

            public static EventIds TimerCancellationFailedIds(ref long eventId)
            {
                const long totalEvents = 1;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Failed, eventId},
                };
                return new EventIds(ids);
            }

            public static EventIds TimerStartedIds(ref long eventId)
            {
                const long totalEvents = 1;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Started, eventId},
                };
                return new EventIds(ids);
            }
            public static EventIds SchedulingFailedIds(ref long eventId)
            {
                const long totalEvents = 1;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Failed, eventId},
                };
                return new EventIds(ids);
            }

            public static EventIds ScheduledIds(ref long eventId)
            {

                const long totalEvents = 1;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Scheduled, eventId},
                };
                return new EventIds(ids);
            }

            public static EventIds StartedIds(ref long eventId)
            {

                const long totalEvents = 2;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Started, eventId},
                    {Scheduled, eventId-1},
                };
                return new EventIds(ids);
            }

            public static EventIds ActivityCancelRequestedIds(ref long eventId)
            {
                const long totalEvents = 3;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {CancelRequested, eventId},
                    {Started, eventId - 1},
                    {Scheduled, eventId - 2}
                };
                return new EventIds(ids);
            }

            public static EventIds GenericEventIds(ref long eventId)
            {
                const long totalEvents = 1;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Generic, eventId},
                };
                return new EventIds(ids);
            }

            public static EventIds ChildWorkflowCancelledIds(ref long eventId)
            {
                const long totalEvents = 5;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Cancelled, eventId},
                    {CancelRequested, eventId-1},
                    {CancelInitiated, eventId-2},
                    {Started, eventId - 3},
                    {Scheduled, eventId - 4}
                };
                return new EventIds(ids);
            }

            public static EventIds ChildWorkflowStartFailed(ref long eventId)
            {
                const long totalEvents = 2;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {StartFailed, eventId},
                    {Scheduled, eventId - 1}
                };
                return new EventIds(ids);
            }

            public static EventIds WorkflowCancelRequestedIds(ref long eventId)
            {
                const long totalEvents = 4;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {CancelRequested, eventId},
                    {CancelInitiated, eventId - 1},
                    {Started, eventId - 2},
                    {Scheduled, eventId - 3}
                };
                return new EventIds(ids);
            }

            public static EventIds ChildWorkflowCancelRequestFailedIds(ref long eventId)
            {
                const long totalEvents = 4;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {CancelRequestFailed, eventId},
                    {CancelInitiated, eventId - 1},
                    {Started, eventId - 2},
                    {Scheduled, eventId - 3}
                };
                return new EventIds(ids);
            }

            public static EventIds WorkflowCancelRequestFailedIds(ref long eventId)
            {
                const long totalEvents = 2;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {CancelRequestFailed, eventId},
                    {CancelInitiated, eventId - 1},
                };
                return new EventIds(ids);
            }
        }
    }
}