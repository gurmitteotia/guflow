// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;

namespace Guflow.Decider
{
    /// <summary>
    /// Fluent API to schedule the Lambda function in workflow.
    /// </summary>
    public interface IFluentLambdaItem : IFluentWorkflowItem<IFluentLambdaItem>
    {
        /// <summary>
        /// Provide the input for lamdba lambda function. Complex input type are serialized to JSON while for primitive types their string represenation is used.
        /// </summary>
        /// <param name="input">Functor, which will return input for lambda function.</param>
        /// <returns></returns>
        IFluentLambdaItem WithInput(Func<ILambdaItem, object> input);

        /// <summary>
        /// Configure the timeout for lambda function. If lambda function does not finish within this period then SWF will raise <see cref="LambdaTimedoutEvent"/>.
        /// </summary>
        /// <param name="timout">Functor, which will return the timeout for lambda function</param>
        /// <returns></returns>
        IFluentLambdaItem WithTimeout(Func<ILambdaItem, TimeSpan?> timout);

        /// <summary>
        /// Provide the workflow action for when lambda function is successfully completed. By default Guflow will schedule its children.
        /// </summary>
        /// <param name="completedAction">Functor, which will return the workflow action.</param>
        /// <returns></returns>
        IFluentLambdaItem OnCompletion(Func<LambdaCompletedEvent, WorkflowAction> completedAction);

        /// <summary>
        /// Provide the workflow action for when lambda function is failed.  By default Guflow will fail the workflow on <see cref="LambdaFailedEvent"/> event.
        /// </summary>
        /// <param name="failedAction"></param>
        /// <returns></returns>
        IFluentLambdaItem OnFailure(Func<LambdaFailedEvent, WorkflowAction> failedAction);

        /// <summary>
        /// Provide the workflow action for when lambda function's execution is timedout. By default Guflow will fail the workflow on <see cref="LambdaTimedoutEvent"/> event.
        /// </summary>
        /// <param name="timedoutAction"></param>
        /// <returns></returns>
        IFluentLambdaItem OnTimedout(Func<LambdaTimedoutEvent, WorkflowAction> timedoutAction);

        /// <summary>
        /// Provide the workflow action for lamdba scheduling failed event. By default 
        /// </summary>
        /// <param name="schedulingFailedAction"></param>
        IFluentLambdaItem OnSchedulingFailed(Func<LambdaSchedulingFailedEvent, WorkflowAction> schedulingFailedAction);

        /// <summary>
        /// Provide the workflow action for when SWF is failed to start the execution of the lamdba function. By default workflow is failed.
        /// </summary>
        /// <param name="startFailedAction"></param>
        /// <returns></returns>
        IFluentLambdaItem OnStartFailed(Func<LambdaStartFailedEvent, WorkflowAction> startFailedAction);

        /// <summary>
        /// Schedule the lambda function only when given expression is evaluated to true.
        /// </summary>
        /// <param name="true">Provide functor to evaluate when scheduling the lambda.</param>
        /// <returns></returns>
        IFluentLambdaItem When(Func<ILambdaItem, bool> @true);

        /// <summary>
        /// Schedule the lambda function only when given expression is evaluated to true.
        /// You also have the option to override triggering workflow action. Refer to Deflow algorithm for more details.
        /// </summary>
        /// <param name="true">Provide functor to evaluate when scheduling the lambda.</param>
        /// <param name="onFalseAction">Provide WorkflowAction when scheduling condition is evaluated to false. By default it will try to schedule the first join item if this is a non startup item.</param>
        /// <returns></returns>
        IFluentLambdaItem When(Func<ILambdaItem, bool> @true, Func<ILambdaItem,WorkflowAction> onFalseAction);
    }
}