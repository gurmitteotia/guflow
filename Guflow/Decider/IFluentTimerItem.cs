using System;

namespace Guflow.Decider
{
    public interface IFluentTimerItem : IFluentWorkflowItem<IFluentTimerItem>
    {
        /// <summary>
        /// Configure the timer to fire after given timespan.
        /// </summary>
        /// <param name="time">Timespan, after which timer will be fired.</param>
        /// <returns></returns>
        IFluentTimerItem FireAfter(TimeSpan time);
        /// <summary>
        /// Provide an expression which workflow will evaulate before scheduling this timer. If it is evaulated to false then timer will not be scheduled.
        /// </summary>
        /// <param name="true"></param>
        /// <returns></returns>
        IFluentTimerItem When(Func<ITimerItem, bool> @true);
        /// <summary>
        /// Provide an expression which workflow will evaulate before scheduling this timer. If it is evaulated to false then timer will not be scheduled.
        /// You also have the option to what happen when condition is evaulated to false. Refer to Deflow algorithm for more details.
        /// </summary>
        /// <param name="true"></param>
        /// <param name="falseAction">WorkflowAction when expression is evaluated to be false.</param>
        /// <returns></returns>
        IFluentTimerItem When(Func<ITimerItem, bool> @true, Func<ITimerItem,WorkflowAction> falseAction);
        /// <summary>
        /// Provide a handler to be called back when timer is fired.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> action);
        /// <summary>
        /// Provide a handler to be called when timer is cancelled.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentTimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> action);
        /// <summary>
        /// Provide a handler to be called back when cancellation requestion to timer is failed.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentTimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> action);
        /// <summary>
        /// Provide a handler to be called back when timer has failed to start in Amazon SWF.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentTimerItem OnStartFailure(Func<TimerStartFailedEvent, WorkflowAction> action);
    }
}