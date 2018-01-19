// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;

namespace Guflow.Decider
{
    internal interface IWorkflow : IWorkflowDefaultActions
    {
        IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem workflowItem);
        WorkflowItem FindWorkflowItemBy(Identity identity);
        void SetCurrentExecutingEvent(WorkflowEvent workflowEvent);
        IWorkflowHistoryEvents WorkflowHistoryEvents { get; }
        WorkflowAction OnWorkflowStarted(WorkflowStartedEvent workflowStartedEvent);
        WorkflowAction OnActivityCompletion(ActivityCompletedEvent activityCompletedEvent);
        WorkflowAction OnActivityFailure(ActivityFailedEvent activityFailedEvent);
        WorkflowAction OnActivityTimeout(ActivityTimedoutEvent activityTimedoutEvent);
        WorkflowAction OnActivityCancelled(ActivityCancelledEvent activityCancelledEvent);
        WorkflowAction OnTimerFired(TimerFiredEvent timerFiredEvent);
        WorkflowAction OnTimerStartFailure(TimerStartFailedEvent timerStartFailedEvent);
        WorkflowAction OnTimerCancelled(TimerCancelledEvent timerCancelledEvent);
        WorkflowAction OnActivityCancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent);
        WorkflowAction OnTimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent);
        WorkflowAction OnActivitySchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent);
        WorkflowAction OnWorkflowSignaled(WorkflowSignaledEvent workflowSignaledEvent);
        WorkflowAction OnWorkflowCancellationRequested(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent);
        WorkflowAction OnRecordMarkerFailed(RecordMarkerFailedEvent recordMarkerFailedEvent);
        WorkflowAction OnWorkflowSignalFailed(WorkflowSignalFailedEvent workflowSignalFailedEvent);
        WorkflowAction OnWorkflowCompletionFailed(WorkflowCompletionFailedEvent workflowCompletionFailedEvent);
        WorkflowAction OnWorkflowFailureFailed(WorkflowFailureFailedEvent workflowFailureFailedEvent);
        WorkflowAction OnWorkflowCancelRequestFailed(WorkflowCancelRequestFailedEvent workflowCancelRequestFailedEvent);
        WorkflowAction OnWorkflowCancellationFailed(WorkflowCancellationFailedEvent workflowCancellationFailedEvent);
    }
}