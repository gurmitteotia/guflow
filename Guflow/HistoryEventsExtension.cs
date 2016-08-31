using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal static class HistoryEventsExtension
    {
        public static bool IsActivityCompletedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskCompleted;
        }

        public static bool IsActivityFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskFailed;
        }

        public static bool IsActivityTimedoutEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskTimedOut;
        }

        public static bool IsActivityCancelledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskCanceled;
        }
        public static bool IsActivityCancellationFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.RequestCancelActivityTaskFailed;
        }
        public static bool IsActivityScheduledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskScheduled;
        }

        public static bool IsActivitySchedulingFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ScheduleActivityTaskFailed;
        }
        public static bool IsActivityStartedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskStarted;
        }

        public static bool IsActivityCancelRequestedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskCancelRequested;
        }
        public static bool IsTimerFiredEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.TimerFired;
        }

        public static bool IsTimerStartedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.TimerStarted;
        }

        public static bool IsTimerStartFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.StartTimerFailed;
        }

        public static bool IsTimerCancelledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.TimerCanceled;
        }

        public static bool IsTimerCancellationFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.CancelTimerFailed;
        }
        public static bool IsWorkflowSignaledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.WorkflowExecutionSignaled;
        }
        public static bool IsWorkflowCancellationRequestedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.WorkflowExecutionCancelRequested;
        }
        public static bool IsWorkflowCompletionFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.CompleteWorkflowExecutionFailed;
        }

        public static bool IsWorkflowFailureFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.FailWorkflowExecutionFailed;
        }

        public static bool IsWorkflowSignalFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.SignalExternalWorkflowExecutionFailed;
        }
        public static bool IsWorkflowStartedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.WorkflowExecutionStarted;
        }

        private static bool IsWorkflowCancelRequestFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.RequestCancelExternalWorkflowExecutionFailed;
        }

        private static bool IsTimerFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.StartTimerFailed;
        }
        public static bool IsActivityStartedEventFor(this HistoryEvent historyEvent, long startedEventId)
        {
            return historyEvent.EventType == EventType.ActivityTaskStarted && historyEvent.EventId == startedEventId;
        }

        public static bool IsActivityScheduledEventFor(this HistoryEvent historyEvent, long scheduledEventId)
        {
            return historyEvent.EventType == EventType.ActivityTaskScheduled && historyEvent.EventId == scheduledEventId;
        }

        public static bool IsTimerStartedEventFor(this HistoryEvent historyEvent, long timerStartedEventId)
        {
            return historyEvent.EventType == EventType.TimerStarted && historyEvent.EventId == timerStartedEventId;
        }

        public static bool IsMarkerRecordedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.MarkerRecorded;
        }
        public static WorkflowEvent CreateInterpretableEvent(this HistoryEvent historyEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            if (historyEvent.IsActivityCompletedEvent())
                return new ActivityCompletedEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsActivityFailedEvent())
                return new ActivityFailedEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsActivityTimedoutEvent())
                return new ActivityTimedoutEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsActivityCancelledEvent())
                return new ActivityCancelledEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsActivityCancellationFailedEvent())
                return new ActivityCancellationFailedEvent(historyEvent);
            if(historyEvent.IsWorkflowStartedEvent())
                return new WorkflowStartedEvent(historyEvent);
            if(historyEvent.IsTimerFiredEvent())
                return new TimerFiredEvent(historyEvent,allHistoryEvents);
            if(historyEvent.IsTimerFailedEvent())
                return new TimerStartFailedEvent(historyEvent);
            if (historyEvent.IsTimerCancelledEvent())
                return new TimerCancelledEvent(historyEvent,allHistoryEvents);
            if (historyEvent.IsTimerCancellationFailedEvent())
                return new TimerCancellationFailedEvent(historyEvent);
            if(historyEvent.IsWorkflowSignaledEvent())
                return new WorkflowSignaledEvent(historyEvent);
            if(historyEvent.IsWorkflowCancellationRequestedEvent())
                return new WorkflowCancellationRequestedEvent(historyEvent);
            if(historyEvent.IsWorkflowCompletionFailedEvent())
                return new WorkflowCompletionFailedEvent(historyEvent);
            if (historyEvent.IsWorkflowFailureFailedEvent())
                return new WorkflowFailureFailedEvent(historyEvent);
            if (historyEvent.IsWorkflowSignalFailedEvent())
                return new WorkflowSignalFailedEvent(historyEvent);
            if (historyEvent.IsWorkflowCancelRequestFailedEvent())
                return new WorkflowCancelRequestFailedEvent(historyEvent);
            return null;
        }

        public static WorkflowItemEvent CreateActivityEventFor(this HistoryEvent historyEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            if (historyEvent.IsActivityCompletedEvent())
                return new ActivityCompletedEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsActivityFailedEvent())
                return new ActivityFailedEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsActivityTimedoutEvent())
                return new ActivityTimedoutEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsActivityCancelledEvent())
                return new ActivityCancelledEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsActivityCancelRequestedEvent())
                return new ActivityCancelRequestedEvent(historyEvent);
            if (historyEvent.IsActivityCancellationFailedEvent())
                return new ActivityCancellationFailedEvent(historyEvent);
            if (historyEvent.IsActivityStartedEvent())
                return new ActivityStartedEvent(historyEvent,allHistoryEvents);
            if (historyEvent.IsActivityScheduledEvent())
                return new ActivityScheduledEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsActivitySchedulingFailedEvent())
                return new ActivitySchedulingFailedEvent(historyEvent);
            return WorkflowItemEvent.NotFound;
        }

        public static WorkflowItemEvent CreateTimerEventFor(this HistoryEvent historyEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            if (historyEvent.IsTimerFiredEvent())
                return new TimerFiredEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsTimerStartedEvent())
                return new TimerStartedEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsTimerStartFailedEvent())
                return  new TimerStartFailedEvent(historyEvent);
            if (historyEvent.IsTimerCancelledEvent())
                return new TimerCancelledEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsTimerCancellationFailedEvent())
                return new TimerCancellationFailedEvent(historyEvent);
            return WorkflowItemEvent.NotFound;
        }

        public static WorkflowItemEvent CreateWorkflowItemEventFor(this HistoryEvent historyEvent,IEnumerable<HistoryEvent> allHistoryEvents)
        {
            var activityEvent = historyEvent.CreateActivityEventFor(allHistoryEvents);
            return activityEvent!=WorkflowItemEvent.NotFound ? activityEvent: historyEvent.CreateTimerEventFor(allHistoryEvents);
        }
    }
}