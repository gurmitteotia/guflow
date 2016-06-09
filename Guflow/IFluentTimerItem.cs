using System;

namespace Guflow
{
    public interface IFluentTimerItem : IFluentWorkflowItem<IFluentTimerItem>
    {
        IFluentTimerItem FireAfter(TimeSpan fireAfter);
        IFluentTimerItem When(Func<ITimerItem, bool> whenFunc);
        IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> onFiredAction);
        IFluentTimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> onCancelledAction);
        IFluentTimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> onCancellationFailedAction);
    }
}