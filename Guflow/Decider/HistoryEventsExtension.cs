// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal static class HistoryEventsExtension
    {
        private static bool IsActivityCompletedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskCompleted;
        }

        private static bool IsActivityFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskFailed;
        }

        private static bool IsActivityTimedoutEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskTimedOut;
        }

        private static bool IsActivityCancelledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskCanceled;
        }

        private static bool IsActivityCancellationFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.RequestCancelActivityTaskFailed;
        }

        private static bool IsActivityScheduledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskScheduled;
        }

        private static bool IsActivitySchedulingFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ScheduleActivityTaskFailed;
        }

        private static bool IsActivityStartedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskStarted;
        }

        private static bool IsActivityCancelRequestedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskCancelRequested;
        }

        private static bool IsTimerFiredEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.TimerFired;
        }

        private static bool IsTimerStartedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.TimerStarted;
        }

        private static bool IsTimerStartFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.StartTimerFailed;
        }

        private static bool IsTimerCancelledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.TimerCanceled;
        }

        private static bool IsTimerCancellationFailedEvent(this HistoryEvent historyEvent)
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

        private static bool IsWorkflowCompletionFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.CompleteWorkflowExecutionFailed;
        }

        private static bool IsWorkflowRestartFailedEvent(this HistoryEvent historyEvent)
            => historyEvent.EventType == EventType.ContinueAsNewWorkflowExecutionFailed;

        private static bool IsWorkflowFailureFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.FailWorkflowExecutionFailed;
        }

        private static bool IsWorkflowSignalFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.SignalExternalWorkflowExecutionFailed;
        }
        public static bool IsWorkflowStartedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.WorkflowExecutionStarted;
        }

        public static bool IsChildWorkflowInitiatedEvent(this HistoryEvent historyEvent, long initiatedEventId)
        {
            return historyEvent.EventType == EventType.StartChildWorkflowExecutionInitiated &&
                   historyEvent.EventId == initiatedEventId;
        }

        private static bool IsWorkflowCancelRequestFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.RequestCancelExternalWorkflowExecutionFailed;
        }
        private static bool IsWorkflowCancellationFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.CancelWorkflowExecutionFailed;
        }
        private static bool IsTimerFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.StartTimerFailed;
        }
        public static bool IsActivityStartedEvent(this HistoryEvent historyEvent, long startedEventId)
        {
            return historyEvent.EventType == EventType.ActivityTaskStarted && historyEvent.EventId == startedEventId;
        }

        public static bool IsActivityScheduledEvent(this HistoryEvent historyEvent, long scheduledEventId)
        {
            return historyEvent.EventType == EventType.ActivityTaskScheduled && historyEvent.EventId == scheduledEventId;
        }

        public static bool IsTimerStartedEvent(this HistoryEvent historyEvent, long timerStartedEventId)
        {
            return historyEvent.EventType == EventType.TimerStarted && historyEvent.EventId == timerStartedEventId;
        }

        public static bool IsMarkerRecordedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.MarkerRecorded;
        }

        public static bool IsLambdaScheduledEvent(this HistoryEvent historyEvent, long scheduledEventId)
        {
            return historyEvent.EventType == EventType.LambdaFunctionScheduled &&
                   historyEvent.EventId == scheduledEventId;
        }

        private static bool IsLambdaCompletedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.LambdaFunctionCompleted;
        }
        private static bool IsLambdaFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.LambdaFunctionFailed;
        }
        private static bool IsLambdaTimedoutEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.LambdaFunctionTimedOut;
        }
        private static bool IsLambdaStartedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.LambdaFunctionStarted;
        }

        private static bool IsLambdaStartFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.StartLambdaFunctionFailed;
        }
        private static bool IsLambdaScheduledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.LambdaFunctionScheduled;
        }
        private static bool IsLambdaSchedulingFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ScheduleLambdaFunctionFailed;
        }

        private static bool IsWorkflowItemSignalledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.MarkerRecorded &&
                   historyEvent.MarkerRecordedEventAttributes.MarkerName == InternalMarkerNames.WorkflowItemSignalled;
        }

        public static WorkflowItemSignalledEvent WorkflowItemSignalledEvent(this HistoryEvent historyEvent)
        {
            if(historyEvent.IsWorkflowItemSignalledEvent())
                return new WorkflowItemSignalledEvent(historyEvent);
            return null;
        }

        private static bool IsWaitForSignalsEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.MarkerRecorded &&
                   historyEvent.MarkerRecordedEventAttributes.MarkerName == InternalMarkerNames.WorkflowItemWaitForSignals;
        }

        public static WaitForSignalsEvent WaitForSignalsEvent(this HistoryEvent historyEvent, IEnumerable<HistoryEvent> allEvents)
        {
            if(historyEvent.IsWaitForSignalsEvent())
                return new WaitForSignalsEvent(historyEvent, allEvents);
            return null;
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
            if (historyEvent.IsActivitySchedulingFailedEvent())
                return new ActivitySchedulingFailedEvent(historyEvent);
            if(historyEvent.IsWorkflowStartedEvent())
                return new WorkflowStartedEvent(historyEvent);
            if(historyEvent.IsTimerFiredEvent())
                return new TimerFiredEvent(historyEvent,allHistoryEvents);
            if(historyEvent.IsTimerFailedEvent())
                return new TimerStartFailedEvent(historyEvent);
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
                return new ExternalWorkflowCancelRequestFailedEvent(historyEvent);
            if (historyEvent.IsWorkflowCancellationFailedEvent())
                return new WorkflowCancellationFailedEvent(historyEvent);
            if (historyEvent.IsLambdaCompletedEvent())
                return new LambdaCompletedEvent(historyEvent, allHistoryEvents);
            if (historyEvent.IsLambdaFailedEvent())
                return new LambdaFailedEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsLambdaTimedoutEvent())
                return new LambdaTimedoutEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsLambdaSchedulingFailedEvent())
                return new LambdaSchedulingFailedEvent(historyEvent);
            if(historyEvent.IsLambdaStartFailedEvent())
                return new LambdaStartFailedEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsChildWorkflowCompletedEvent())
                return new ChildWorkflowCompletedEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsChildWorkflowFailedEvent())
                return new ChildWorkflowFailedEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsChildWorkflowCancelledEvent())
                return new ChildWorkflowCancelledEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsChildWorkflowTimedoutEvent())
                return new ChildWorkflowTimedoutEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsChildWorkflowTerminatedEvent())
                return new ChildWorkflowTerminatedEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsChildWorkflowStartFailedEvent())
                return new ChildWorkflowStartFailedEvent(historyEvent, allHistoryEvents);
            if(historyEvent.IsWorkflowRestartFailedEvent())
                return new WorkflowRestartFailedEvent(historyEvent);
            if(historyEvent.IsChildWorkflowStartedEvent())
                return new ChildWorkflowStartedEvent(historyEvent, allHistoryEvents);
            return null;
        }

        public static WorkflowItemEvent ActivityEvent(this HistoryEvent historyEvent, IEnumerable<HistoryEvent> allHistoryEvents)
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
            return null;
        }

        public static WorkflowItemEvent TimerEvent(this HistoryEvent historyEvent, IEnumerable<HistoryEvent> allHistoryEvents)
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
            return null;
        }

        public static WorkflowItemEvent LambdaEvent(this HistoryEvent historyEvent, IEnumerable<HistoryEvent> allEvents)
        {
            if(historyEvent.IsLambdaCompletedEvent())
                return new LambdaCompletedEvent(historyEvent, allEvents);
            if(historyEvent.IsLambdaFailedEvent())
                return new LambdaFailedEvent(historyEvent, allEvents);
            if (historyEvent.IsLambdaTimedoutEvent())
                return new LambdaTimedoutEvent(historyEvent, allEvents);
            if (historyEvent.IsLambdaStartedEvent())
                return new LambdaStartedEvent(historyEvent, allEvents);
            if (historyEvent.IsLambdaStartFailedEvent())
                return new LambdaStartFailedEvent(historyEvent, allEvents);
            if (historyEvent.IsLambdaScheduledEvent())
                return new LambdaScheduledEvent(historyEvent);
            if (historyEvent.IsLambdaSchedulingFailedEvent())
                return new LambdaSchedulingFailedEvent(historyEvent);

            return null;
        }

        public static WorkflowItemEvent ChildWorkflowEvent(this HistoryEvent historyEvent, IEnumerable<HistoryEvent> allEvents)
        {
            if(historyEvent.IsChildWorkflowCompletedEvent())
                return new ChildWorkflowCompletedEvent(historyEvent, allEvents);
            if (historyEvent.IsChildWorkflowFailedEvent())
                return new ChildWorkflowFailedEvent(historyEvent, allEvents);
            if (historyEvent.IsChildWorkflowCancelledEvent())
                return new ChildWorkflowCancelledEvent(historyEvent, allEvents);
            if (historyEvent.IsChildWorkflowTimedoutEvent())
                return new ChildWorkflowTimedoutEvent(historyEvent, allEvents);
            if (historyEvent.IsChildWorkflowTerminatedEvent())
                return new ChildWorkflowTerminatedEvent(historyEvent, allEvents);
            if (historyEvent.IsChildWorkflowStartedEvent())
                return new ChildWorkflowStartedEvent(historyEvent, allEvents);
            if (historyEvent.IsChildWorkflowStartFailedEvent())
                return new ChildWorkflowStartFailedEvent(historyEvent, allEvents);
            if (historyEvent.IsExternalWorkflowCancelRequestedEvent())
                return new ExternalWorkflowCancellationRequestedEvent(historyEvent);
            if (historyEvent.IsExternalWorkflowCancelRequestFailedEvent())
                return new ExternalWorkflowCancelRequestFailedEvent(historyEvent);
            return null;
        }

        private static bool IsChildWorkflowCompletedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ChildWorkflowExecutionCompleted;
        }
        private static bool IsChildWorkflowFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ChildWorkflowExecutionFailed;
        }
        private static bool IsChildWorkflowCancelledEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ChildWorkflowExecutionCanceled;
        }
        private static bool IsChildWorkflowTimedoutEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ChildWorkflowExecutionTimedOut;
        }
        private static bool IsChildWorkflowTerminatedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ChildWorkflowExecutionTerminated;
        }
        private static bool IsChildWorkflowStartedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ChildWorkflowExecutionStarted;
        }
        private static bool IsChildWorkflowStartFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.StartChildWorkflowExecutionFailed;
        }
        private static bool IsExternalWorkflowCancelRequestedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ExternalWorkflowExecutionCancelRequested;
        }
        private static bool IsExternalWorkflowCancelRequestFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.RequestCancelExternalWorkflowExecutionFailed;
        }

        public static WorkflowItemEvent CreateWorkflowItemEventFor(this HistoryEvent historyEvent,IEnumerable<HistoryEvent> allHistoryEvents)
        {
            var itemEvent = historyEvent.ActivityEvent(allHistoryEvents);
            itemEvent = itemEvent ?? historyEvent.TimerEvent(allHistoryEvents);
            itemEvent = itemEvent ?? historyEvent.ChildWorkflowEvent(allHistoryEvents);
            itemEvent = itemEvent ?? historyEvent.LambdaEvent(allHistoryEvents);
            return itemEvent;
        }
    }
}