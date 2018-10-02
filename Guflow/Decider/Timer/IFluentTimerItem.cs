// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
        /// Configure the timer to fire after given timespan. It will take priority over other FireAfter overload.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        IFluentTimerItem FireAfter(Func<ITimerItem, TimeSpan> time);

        /// <summary>
        /// Provide an expression which workflow will evaluate before scheduling this timer. If it is evaluated to false then timer will not be scheduled.
        /// </summary>
        /// <param name="true"></param>
        /// <returns></returns>
        IFluentTimerItem When(Func<ITimerItem, bool> @true);
        /// <summary>
        /// Provide an expression which workflow will evaluate before scheduling this timer. If it is evaluated to false then timer will not be scheduled.
        /// You also have the option to what happen when condition is evaluated to false. Refer to Deflow algorithm for more details.
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
        /// Provide a handler to be called back when cancellation request to timer is failed.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentTimerItem OnCancellationFailed(Func<TimerCancellationFailedEvent, WorkflowAction> action);
        /// <summary>
        /// Provide a handler to be called back when timer has failed to start in Amazon SWF.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentTimerItem OnStartFailed(Func<TimerStartFailedEvent, WorkflowAction> action);
        /// <summary>
        /// Provide a handler to be called back when the cancel request for timer is sent to Amazon SWF. This handler is only invoked if the timer is active.
        /// </summary>
        /// <param name="action">WorkflowAction to be executed when timer is cancelled.</param>
        /// <returns></returns>
        IFluentTimerItem OnCancel(Func<ITimerItem, WorkflowAction> action);
        
    }
}