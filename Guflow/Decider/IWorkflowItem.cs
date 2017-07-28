using System.Collections.Generic;

namespace Guflow.Decider
{
    public interface IWorkflowItem
    {
        IEnumerable<IActivityItem> ParentActivities { get; }
        IEnumerable<ITimerItem> ParentTimers{ get; }
        WorkflowItemEvent LastEvent { get; }
        IEnumerable<WorkflowItemEvent> AllEvents { get; }
        bool IsActive { get; }
    }

    internal interface ITimer
    {
        WorkflowAction Fired(TimerFiredEvent timerFiredEvent);
        WorkflowAction Cancelled(TimerCancelledEvent timerCancelledEvent);
        WorkflowAction StartFailed(TimerStartFailedEvent timerStartFailedEvent);
        WorkflowAction CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent);
    }
    internal interface IActivity
    {
        WorkflowAction Completed(ActivityCompletedEvent activityCompletedEvent);
        WorkflowAction Failed(ActivityFailedEvent activityFailedEvent);
        WorkflowAction Timedout(ActivityTimedoutEvent activityTimedoutEvent);
        WorkflowAction Cancelled(ActivityCancelledEvent activityCancelledEvent);
        WorkflowAction CancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent);
        WorkflowAction SchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent);
    }
}