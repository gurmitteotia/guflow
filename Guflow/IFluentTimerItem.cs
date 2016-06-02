using System;

namespace Guflow
{
    public interface IFluentTimerItem
    {
        IFluentTimerItem FireAfter(TimeSpan fireAfter);
        IFluentTimerItem When(Func<ITimerItem, bool> whenFunc);
        IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> onFiredAction);
        IFluentTimerItem DependsOn(string timerName);
        IFluentTimerItem DependsOn(string activityName, string activityVersion, string positionalName = "");
        IFluentTimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> onCancelledAction);
        IFluentTimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> onCancellationFailedAction);
    }
}