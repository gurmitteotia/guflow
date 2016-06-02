using System;

namespace Guflow
{
    public interface IFluentActivityItem
    {
        IFluentActivityItem DependsOn(string name, string version, string positionalName = "");
        IFluentActivityItem DependsOn(string timerName);
        IFluentActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> onCompletionFunc);
        IFluentActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> onFailureFunc);
        IFluentActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> onTimedoutFunc);
        IFluentActivityItem OnTimerCancelled(Func<TimerCancelledEvent, WorkflowAction> onTimerCancelledAction);
        IFluentActivityItem OnFailedCancellation(Func<ActivityCancellationFailedEvent, WorkflowAction> onFailedCancellationAction);
        IFluentActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> onCancelledFunc);
        IFluentActivityItem WithInput(Func<IActivityItem, string> inputFunc);
        IFluentActivityItem OnTaskList(Func<IActivityItem, string> taskListFunc);
        IFluentActivityItem When(Func<IActivityItem, bool> whenFunc);
        IFluentActivityItem WithPriority(Func<IActivityItem, int?> priorityFunc);
        IFluentActivityItem WithTimeouts(Func<IActivityItem, ScheduleActivityTimeouts> timeoutsFunc);
    }
}