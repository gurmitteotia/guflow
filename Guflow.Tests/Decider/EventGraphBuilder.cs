// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using ChildPolicy = Guflow.Decider.ChildPolicy;

namespace Guflow.Tests.Decider
{
    internal class EventGraphBuilder
    {
        private long _currentEventId = 0;
        public IEnumerable<HistoryEvent> ActivityCompletedGraph(Identity activityIdentity, string workerIdentity, string result, string input = "")
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
                    ActivityId = activityIdentity.Id,
                    Input = input
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityFailedGraph(Identity activityIdentity, string identity, string reason, string detail)
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
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityTimedoutGraph(Identity activityIdentity, string identity, string timeoutType, string detail)
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
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityCancelledGraph(Identity activityIdentity, string identity, string detail)
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
                    ActivityId = activityIdentity.Id,
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
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerFiredGraph(Identity timerId, TimeSpan startToFireTimeout, bool isARescheduleTimer = false)
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
                    TimerId = timerId.Id
                },
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.EventId(EventIds.Started),
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId.Id,
                    StartToFireTimeout = ((long)startToFireTimeout.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, IsARescheduleTimer = isARescheduleTimer }).ToJson()
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerCancelledGraph(Identity timerId, TimeSpan startToFireTimeout, bool isARescheduleTimer = false)
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
                    TimerId = timerId.Id,
                },
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.EventId(EventIds.Started),
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId.Id,
                    StartToFireTimeout = ((long)startToFireTimeout.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, IsARescheduleTimer = isARescheduleTimer }).ToJson()
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerStartFailedGraph(Identity timerId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimerStartFailedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.StartTimerFailed,
                EventId = eventIds.EventId(EventIds.Failed),
                StartTimerFailedEventAttributes = new StartTimerFailedEventAttributes()
                {
                    TimerId = timerId.Id,
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
        public IEnumerable<HistoryEvent> ActivityCancellationFailedGraph(Identity activityId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CompletedIds(ref _currentEventId);


            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.RequestCancelActivityTaskFailed,
                EventId = eventIds.EventId(EventIds.Completion),
                RequestCancelActivityTaskFailedEventAttributes = new RequestCancelActivityTaskFailedEventAttributes()
                {
                    ActivityId = activityId.Id,
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
                    ActivityId = activityId.Id,
                    Control = (new ScheduleData() { PN = activityId.PositionalName }).ToJson(),
                }
            });
            return historyEvents;
        }
        public IEnumerable<HistoryEvent> TimerCancellationFailedGraph(Identity timerId, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CancellationFailedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.CancelTimerFailed,
                EventId = eventIds.EventId(EventIds.Failed),
                CancelTimerFailedEventAttributes = new CancelTimerFailedEventAttributes()
                {
                    TimerId = timerId.Id,
                    Cause = cause
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.EventId(EventIds.Started),
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = timerId.Id,
                    StartToFireTimeout = ((long)20).ToString(),
                    Control = (new TimerScheduleData() { TimerName = timerId.Name, IsARescheduleTimer = false }).ToJson()
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> TimerStartedGraph(Identity identity, TimeSpan fireAfter, bool isARescheduleTimer = false)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.TimerStartedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.TimerStarted,
                EventId = eventIds.EventId(EventIds.Started),
                TimerStartedEventAttributes = new TimerStartedEventAttributes()
                {
                    TimerId = identity.Id,
                    StartToFireTimeout = ((long)fireAfter.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerName = identity.Name, IsARescheduleTimer = isARescheduleTimer }).ToJson()
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivitySchedulingFailedGraph(Identity activityIdentity, string cause)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.SchedulingFailedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ScheduleActivityTaskFailed,
                EventId = eventIds.EventId(EventIds.Failed),
                ScheduleActivityTaskFailedEventAttributes = new ScheduleActivityTaskFailedEventAttributes()
                {
                    ActivityId = activityIdentity.Id,
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Cause = cause
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityScheduledGraph(Identity activityIdentity)
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
                    ActivityId = activityIdentity.Id
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityStartedGraph(Identity activityIdentity, string identity)
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
                    ActivityType = new ActivityType() { Name = activityIdentity.Name, Version = activityIdentity.Version },
                    Control = (new ScheduleData() { PN = activityIdentity.PositionalName }).ToJson(),
                    ActivityId = activityIdentity.Id
                }
            });
            return historyEvents;
        }

        public IEnumerable<HistoryEvent> ActivityCancelRequestedGraph(Identity activityIdentity, string identity)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.ActivityCancelRequestedIds(ref _currentEventId);

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCancelRequested,
                EventId = eventIds.EventId(EventIds.CancelRequested),
                ActivityTaskCancelRequestedEventAttributes = new ActivityTaskCancelRequestedEventAttributes()
                {
                    ActivityId = activityIdentity.Id,
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
                    ActivityId = activityIdentity.Id
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
        public HistoryEvent WorkflowSignaledEvent(string signalName, string input)
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
        public HistoryEvent WorkflowCancelRequestFailedEvent(string cause)
        {
            var eventIds = EventIds.GenericEventIds(ref _currentEventId);
            return new HistoryEvent
            {
                EventId = eventIds.EventId(EventIds.Generic),
                EventType = EventType.RequestCancelExternalWorkflowExecutionFailed,
                RequestCancelExternalWorkflowExecutionFailedEventAttributes = new RequestCancelExternalWorkflowExecutionFailedEventAttributes()
                {
                    Cause = cause,
                }
            };
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

        public IEnumerable<HistoryEvent> Concat(params IEnumerable<HistoryEvent>[] graphs)
        {
            var result = new List<HistoryEvent>();
            foreach (var graph in graphs)
            {
                result.AddRange(graph);
            }
            result.Reverse();
            return result;
        }

        public IEnumerable<HistoryEvent> LambdaCompletedEventGraph(Identity identity, object input, object result, TimeSpan? startToClose = null)
        {
            var historyEvents = new List<HistoryEvent>();
            var eventIds = EventIds.CompletedIds(ref _currentEventId);
            historyEvents.Add(new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Completion),
                EventType = EventType.LambdaFunctionCompleted,
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
                    Id = identity.Id,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = startToClose.Seconds()
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> LambdaFailedEventGraph(Identity identity, object input, string reason, string details, TimeSpan? timeout = null)
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
                    Id = identity.Id,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            });

            return historyEvents;
        }

        public IEnumerable<HistoryEvent> LamdbaTimedoutEventGraph(Identity identity, object input, string timedoutType, TimeSpan? timeout = null)
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
                    Id = identity.Id,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            });

            return historyEvents;
        }

        public HistoryEvent LambdaSchedulingFailedEventGraph(Identity identity, string reason)
        {
            var eventIds = EventIds.SchedulingFailedIds(ref _currentEventId);
            return new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Failed),
                EventType = EventType.ScheduleLambdaFunctionFailed,
                ScheduleLambdaFunctionFailedEventAttributes = new ScheduleLambdaFunctionFailedEventAttributes
                {
                    Id = identity.Id,
                    Name = identity.Name,
                    Cause = reason
                }
            };
        }

        public IEnumerable<HistoryEvent> LambdaStartFailedEventGraph(Identity identity, string input, string cause, string message, TimeSpan? timeout = null)
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
                    Id = identity.Id,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            });

            return historyEvents;
        }

        public HistoryEvent LambdaScheduledEventGraph(Identity identity, object input, TimeSpan? timeout = null)
        {
            var eventIds = EventIds.ScheduledIds(ref _currentEventId);
            return new HistoryEvent()
            {
                EventId = eventIds.EventId(EventIds.Scheduled),
                EventType = EventType.LambdaFunctionScheduled,
                LambdaFunctionScheduledEventAttributes = new LambdaFunctionScheduledEventAttributes
                {
                    Id = identity.Id,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    Control = (new ScheduleData() { PN = identity.PositionalName }).ToJson(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            };
        }

        public IEnumerable<HistoryEvent> LambdaStartedEventGraph(Identity identity, object input, TimeSpan? timeout=null)
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
                    Id = identity.Id,
                    Name = identity.Name,
                    Input = input.ToAwsString(),
                    StartToCloseTimeout = timeout.Seconds()
                }
            });

            return historyEvents;
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

            public static EventIds CancellationFailedIds(ref long eventId)
            {
                const long totalEvents = 2;
                eventId += totalEvents;
                var ids = new Dictionary<string, long>()
                {
                    {Failed, eventId},
                    {Started, eventId-1},
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
        }
    }
}