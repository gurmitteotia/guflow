using System;

namespace Guflow.Decider
{
    public interface IFluentTimerItem : IFluentWorkflowItem<IFluentTimerItem>
    {
        IFluentTimerItem FireAfter(TimeSpan time);
        IFluentTimerItem When(Func<ITimerItem, bool> @true);
        IFluentTimerItem When(Func<ITimerItem, bool> @true, Func<ITimerItem,WorkflowAction> falseAction);
        IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> action);
        IFluentTimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> action);
        IFluentTimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> action);
        IFluentTimerItem OnStartFailure(Func<TimerStartFailedEvent, WorkflowAction> action);
    }
}