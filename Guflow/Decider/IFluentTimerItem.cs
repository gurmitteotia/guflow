using System;

namespace Guflow.Decider
{
    public interface IFluentTimerItem : IFluentWorkflowItem<IFluentTimerItem>
    {
        IFluentTimerItem FireAfter(TimeSpan fireAfter);
        IFluentTimerItem When(Func<ITimerItem, bool> whenFunc);
        IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> onFiredAction);
        IFluentTimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> onCancelledFunc);
        IFluentTimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> onCancellationFailedFunc);
        IFluentTimerItem OnStartFailure(Func<TimerStartFailedEvent, WorkflowAction> onStartFailureAction);
    }
}