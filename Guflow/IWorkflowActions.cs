namespace Guflow
{
    internal interface IWorkflowActions
    {
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
    }
}