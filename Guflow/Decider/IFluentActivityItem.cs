using System;

namespace Guflow.Decider
{
    public interface IFluentActivityItem: IFluentWorkflowItem<IFluentActivityItem>
    {
        /// <summary>
        /// Configure a handler to be called back when activity is completed
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> action);
        /// <summary>
        /// Register a handler to be called back when activity is failed.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> action);
        /// <summary>
        /// Register a handler to be called back when activity is timedout out.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> action);
        /// <summary>
        /// Register a handler to be called back when cancellation request to scheduled activity is failed.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentActivityItem OnFailedCancellation(Func<ActivityCancellationFailedEvent, WorkflowAction> action);
        /// <summary>
        /// Register a handler to be called when activity is failed to scheduled.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentActivityItem OnFailedScheduling(Func<ActivitySchedulingFailedEvent, WorkflowAction> action);
        /// <summary>
        /// Register a handler to be called when activity is cancelled.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IFluentActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> action);
        /// <summary>
        /// Provides input to activity. Input will be passed to activity worker. 
        /// </summary>
        /// <param name="data">Input, it is a custom type then it will be serialized to JSON format and for premitive type it directly serialize them to string.</param>
        /// <returns></returns>
        IFluentActivityItem WithInput(Func<IActivityItem, object> data);

        /// <summary>
        /// Provides the task list activity will be scheduled on.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFluentActivityItem OnTaskList(Func<IActivityItem, string> name);

        /// <summary>
        /// Provide an expression which workflow will evaulate before scheduling this activity. If it is evaulated to false then activity will not be scheduled.
        /// </summary>
        /// <param name="true"></param>
        /// <returns></returns>
        IFluentActivityItem When(Func<IActivityItem, bool> @true);
        /// <summary>
        /// Provide an expression which workflow will evaulate before scheduling this activity. If it is evaulated to false then activity will not be scheduled.
        /// You also have the option to what happen when condition is evaulated to false. Refer to Deflow algorithm for more details.
        /// </summary>
        /// <param name="true"></param>
        /// <param name="falseAction">WorkflowAction when expression is evaluated to be false.</param>
        /// <returns></returns>
        IFluentActivityItem When(Func<IActivityItem, bool> @true, Func<IActivityItem, WorkflowAction> falseAction);
        /// <summary>
        /// Configure the scheduling priorioty of activity. It is directly linked to Amazon SWF priority.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        IFluentActivityItem WithPriority(Func<IActivityItem, int?> number);
        /// <summary>
        /// Configure various timeouts while scheduling the activity. This gives you the option to override the various timeouts provided during activity registration.
        /// </summary>
        /// <param name="timeouts"></param>
        /// <returns></returns>
        IFluentActivityItem WithTimeouts(Func<IActivityItem, ActivityTimeouts> timeouts);
        IFluentTimerItem RescheduleTimer { get; }
    }
}