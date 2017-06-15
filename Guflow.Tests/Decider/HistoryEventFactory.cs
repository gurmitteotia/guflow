﻿using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using ChildPolicy = Guflow.Decider.ChildPolicy;

namespace Guflow.Tests.Decider
{
    internal class HistoryEventFactory
    {
        public static IEnumerable<HistoryEvent> CreateActivityCompletedEventGraph(Identity activityIdentity, string workerIdentity, string result, string input="")
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCompleted,
                EventId = eventIds.CompletedId,
                ActivityTaskCompletedEventAttributes = new ActivityTaskCompletedEventAttributes()
                {
                    Result = result,
                    StartedEventId = eventIds.StartedId,
                    ScheduledEventId = eventIds.ScheduledId
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.StartedId,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = workerIdentity,
                    ScheduledEventId = eventIds.ScheduledId

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ActivityScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity.Id,
                    Input = input
                }
            });
            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateActivityFailedEventGraph(Identity activityIdentity, string identity, string reason, string detail)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskFailed,
                EventId = eventIds.CompletedId,
                ActivityTaskFailedEventAttributes = new ActivityTaskFailedEventAttributes()
                {
                    Details = detail,
                    Reason = reason,
                    ScheduledEventId = eventIds.ScheduledId,
                    StartedEventId = eventIds.StartedId
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.StartedId,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.ScheduledId

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ActivityScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateActivityTimedoutEventGraph(Identity activityIdentity, string identity, string timeoutType, string detail)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskTimedOut,
                EventId = eventIds.CompletedId,
                ActivityTaskTimedOutEventAttributes = new ActivityTaskTimedOutEventAttributes()
                {
                    Details = detail,
                    TimeoutType = new ActivityTaskTimeoutType(timeoutType),
                    ScheduledEventId = eventIds.ScheduledId,
                    StartedEventId = eventIds.StartedId
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.StartedId,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.ScheduledId

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ActivityScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateActivityCancelledEventGraph(Identity activityIdentity, string identity, string detail)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCanceled,
                EventId = eventIds.CompletedId,
                ActivityTaskCanceledEventAttributes = new ActivityTaskCanceledEventAttributes()
                {
                    Details = detail,
                    LatestCancelRequestedEventId = eventIds.CancelRequestedId,
                    ScheduledEventId = eventIds.ScheduledId,
                    StartedEventId = eventIds.StartedId
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCancelRequested,
                EventId = eventIds.CancelRequestedId,
                ActivityTaskCancelRequestedEventAttributes = new ActivityTaskCancelRequestedEventAttributes()
                {
                    ActivityId = activityIdentity.Id
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.StartedId,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.ScheduledId

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ActivityScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateTimerFiredEventGraph(Identity timerId, TimeSpan startToFireTimeout, bool isARescheduleTimer=false)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerFired,
                EventId = eventIds.CompletedId,
                TimerFiredEventAttributes = new TimerFiredEventAttributes()
                {
                    StartedEventId = eventIds.StartedId,
                    TimerId = timerId.Id
                },
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.StartedId,
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId.Id,
                    StartToFireTimeout = ((long)startToFireTimeout.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, IsARescheduleTimer = isARescheduleTimer}).ToJson()
                }
            });

            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateTimerCancelledEventGraph(Identity timerId, TimeSpan startToFireTimeout, bool isARescheduleTimer = false)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerCanceled,
                EventId = eventIds.CompletedId,
                TimerCanceledEventAttributes = new TimerCanceledEventAttributes()
                {
                    StartedEventId = eventIds.StartedId,
                    TimerId = timerId.Id,
                },
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.StartedId,
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId.Id,
                    StartToFireTimeout = ((long)startToFireTimeout.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, IsARescheduleTimer = isARescheduleTimer }).ToJson()
                }
            });

            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateTimerStartFailedEventGraph(Identity timerId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.StartTimerFailed,
                EventId = eventIds.CompletedId,
                StartTimerFailedEventAttributes = new StartTimerFailedEventAttributes()
                {
                    TimerId = timerId.Id,
                    Cause = cause
                },
            });

            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateWorkflowStartedEventGraph(string input ="workflowinput")
        {
            return new[] {CreateWorkflowStartedEvent(input)};
        }

        public static HistoryEvent CreateWorkflowStartedEvent(string input="some input")
        {
            return new HistoryEvent()
            {
                EventType =  EventType.WorkflowExecutionStarted,
                WorkflowExecutionStartedEventAttributes = new WorkflowExecutionStartedEventAttributes()
                {
                    ChildPolicy = ChildPolicy.Terminate,
                    ContinuedExecutionRunId = "continue run id",
                    ExecutionStartToCloseTimeout = "100",
                    Input = input,
                    LambdaRole = "some role",
                    ParentInitiatedEventId = 10,
                    ParentWorkflowExecution = new WorkflowExecution() { RunId = "parent runid", WorkflowId = "parent workflow id" },
                    TagList = new List<string>() { "First", "Second" },
                    TaskList = new TaskList() { Name = "task name" },
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "30",
                }
            };
        }
        public static IEnumerable<HistoryEvent> CreateActivityCancellationFailedEventGraph(Identity activityId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.RequestCancelActivityTaskFailed,
                EventId = eventIds.CompletedId,
                RequestCancelActivityTaskFailedEventAttributes = new RequestCancelActivityTaskFailedEventAttributes()
                {
                    ActivityId = activityId.Id,
                    Cause = cause
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.StartedId,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                   Identity = "someid",
                   ScheduledEventId = eventIds.ScheduledId
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() {  Name = activityId.Name, Version = activityId.Version},
                    ActivityId = activityId.Id,
                    Control = (new ActivityScheduleData() { PN = activityId.PositionalName }).ToJson(),
                }
            });

            return historyEvents;
        }
        public static IEnumerable<HistoryEvent> CreateTimerCancellationFailedEventGraph(Identity timerId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.CancelTimerFailed,
                EventId = eventIds.CompletedId,
                CancelTimerFailedEventAttributes = new CancelTimerFailedEventAttributes()
                {
                    TimerId = timerId.Id,
                    Cause = cause
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.StartedId,
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId.Id,
                    StartToFireTimeout = ((long)20).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, IsARescheduleTimer = false }).ToJson()
                }
            });

            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateTimerStartedEventGraph(Identity identity, TimeSpan fireAfter, bool isARescheduleTimer =false)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.StartedId,
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = identity.Id,
                    StartToFireTimeout = ((long)fireAfter.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = identity.Name, IsARescheduleTimer = isARescheduleTimer }).ToJson()
                }
            });

            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateActivitySchedulingFailedEventGraph(Identity activityIdentity, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ScheduleActivityTaskFailed,
                EventId = eventIds.CompletedId,
                ScheduleActivityTaskFailedEventAttributes = new ScheduleActivityTaskFailedEventAttributes()
                {
                    ActivityId = activityIdentity.Id,
                    ActivityType = new ActivityType() {  Name = activityIdentity.Name, Version = activityIdentity.Version},
                    Cause = cause
                }
            });

            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateActivityScheduledEventGraph(Identity activityIdentity)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ActivityScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity.Id
                }
            });

            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateActivityStartedEventGraph(Identity activityIdentity, string identity)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.StartedId,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.ScheduledId

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ActivityScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }

        public static IEnumerable<HistoryEvent> CreateActivityCancelRequestedGraph(Identity activityIdentity, string identity)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.NewEventIds;

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCancelRequested,
                EventId = eventIds.CancelRequestedId,
                ActivityTaskCancelRequestedEventAttributes = new ActivityTaskCancelRequestedEventAttributes()
                {
                        ActivityId = activityIdentity.Id,
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.StartedId,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.ScheduledId

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ActivityScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }


        public static HistoryEvent CreateWorkflowSignaledEvent(string signalName, string input, string externalWorkflowRunId, string externalWorkflowId)
        {
            var eventIds = EventIds.NewEventIds;

            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.WorkflowExecutionSignaled,
                WorkflowExecutionSignaledEventAttributes = new WorkflowExecutionSignaledEventAttributes()
                {
                    SignalName = signalName,
                    Input = input,
                    ExternalWorkflowExecution = new WorkflowExecution() { RunId  = externalWorkflowRunId, WorkflowId = externalWorkflowId}
                }
            };
        }
        public static HistoryEvent CreateWorkflowSignaledEvent(string signalName, string input)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.WorkflowExecutionSignaled,
                WorkflowExecutionSignaledEventAttributes = new WorkflowExecutionSignaledEventAttributes()
                {
                    SignalName = signalName,
                    Input = input,
                }
            };
        }
        public static HistoryEvent CreateWorkflowCancellationRequestedEvent(string cause, string externalWorkflowRunid,string externalWorkflowId)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType    = EventType.WorkflowExecutionCancelRequested,
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
        public static HistoryEvent CreateWorkflowCancellationRequestedEvent(string cause)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.WorkflowExecutionCancelRequested,
                WorkflowExecutionCancelRequestedEventAttributes = new WorkflowExecutionCancelRequestedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }

        public static HistoryEvent CreateMarkerRecordedEvent(string markerName, string detail1)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.MarkerRecorded,
                MarkerRecordedEventAttributes = new MarkerRecordedEventAttributes()
                {
                    MarkerName = markerName,
                    Details = detail1
                }
            };
        }

        public static HistoryEvent CreateRecordMarkerFailedEvent(string markerName,string cause)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.RecordMarkerFailed,
                RecordMarkerFailedEventAttributes = new RecordMarkerFailedEventAttributes()
                {
                    MarkerName = markerName,
                    Cause = cause
                }
            };
        }

        public static HistoryEvent CreateWorkflowSignalFailedEvent(string cause, string workflowId, string runId)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.SignalExternalWorkflowExecutionFailed,
                SignalExternalWorkflowExecutionFailedEventAttributes = new SignalExternalWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                    WorkflowId = workflowId,
                    RunId = runId
                }
            };
        }

        public static HistoryEvent CreateWorkflowCompletionFailureEvent(string cause)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.CompleteWorkflowExecutionFailed,
                CompleteWorkflowExecutionFailedEventAttributes = new CompleteWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }

        public static HistoryEvent CreateWorkflowFailureFailedEvent(string cause)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.FailWorkflowExecutionFailed,
                FailWorkflowExecutionFailedEventAttributes = new FailWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }
        public static HistoryEvent CreateWorkflowCancelRequestFailedEvent(string cause)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.RequestCancelExternalWorkflowExecutionFailed,
                RequestCancelExternalWorkflowExecutionFailedEventAttributes = new RequestCancelExternalWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }
        public static HistoryEvent CreateWorkflowCancellationFailedEvent(string cause)
        {
            var eventIds = EventIds.NewEventIds;
            return new HistoryEvent
            {
                EventId = eventIds.CompletedId,
                EventType = EventType.CancelWorkflowExecutionFailed,
                CancelWorkflowExecutionFailedEventAttributes = new CancelWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
        }

        private class EventIds
        {
            private static long _seed = 1;
            private readonly long _scheduledId;
            private EventIds(long scheduledId)
            {
                _scheduledId = scheduledId;
            }

            public static EventIds NewEventIds
            {
                get
                {
                    _seed += 10;
                    return new EventIds(_seed);
                }
            }

            public long CompletedId
            {
                get { return _scheduledId + 3; }
            }

            public long CancelRequestedId
            {
                get { return _scheduledId + 2; }
            }
            public long StartedId
            {
                get { return _scheduledId + 1; }
            }

            public long ScheduledId
            {
                get { return _scheduledId; }
            }
        }
    }
}