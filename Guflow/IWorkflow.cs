namespace Guflow
{
    public interface IWorkflow
    {
        WorkflowAction WorkflowStarted(WorkflowStartedEvent workflowStartedEvent);
        WorkflowAction ActivityCompleted(ActivityCompletedEvent activityCompletedEvent);
        WorkflowAction ActivityFailed(ActivityFailedEvent activityFailedEvent);
        WorkflowAction ActivityTimedout(ActivityTimedoutEvent activityTimedoutEvent);
        WorkflowAction ActivityCancelled(ActivityCancelledEvent activityCancelledEvent);
        WorkflowAction TimerFired(TimerFiredEvent timerFiredEvent);
        WorkflowAction TimerFailed(TimerStartFailedEvent timerStartFailedEvent);
        WorkflowAction TimerCancelled(TimerCancelledEvent timerCancelledEvent);
        WorkflowAction ActivityCancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent);
        WorkflowAction TimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent);
        WorkflowAction ActivitySchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent);
    }
}