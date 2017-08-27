using System;

namespace Guflow.Decider
{
    public interface IFluentActivityItem: IFluentWorkflowItem<IFluentActivityItem>
    {
        IFluentActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> action);
        IFluentActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> action);
        IFluentActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> action);
        IFluentActivityItem OnFailedCancellation(Func<ActivityCancellationFailedEvent, WorkflowAction> action);
        IFluentActivityItem OnFailedScheduling(Func<ActivitySchedulingFailedEvent, WorkflowAction> action);
        IFluentActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> action);
        IFluentActivityItem WithInput(Func<IActivityItem, object> data);
        IFluentActivityItem OnTaskList(Func<IActivityItem, string> name);
        IFluentActivityItem When(Func<IActivityItem, bool> @true);
        IFluentActivityItem When(Func<IActivityItem, bool> @true, Func<IActivityItem, WorkflowAction> falseAction);
        IFluentActivityItem WithPriority(Func<IActivityItem, int?> number);
        IFluentActivityItem WithTimeouts(Func<IActivityItem, ScheduleActivityTimeouts> timeouts);
        IFluentTimerItem RescheduleTimer { get; }
    }
}