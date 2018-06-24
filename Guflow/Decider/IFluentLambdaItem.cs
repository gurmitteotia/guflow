// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;

namespace Guflow.Decider
{
    /// <summary>
    /// Fluent API to schedule the Lambda function in workflow.
    /// </summary>
    public interface IFluentLambdaItem : IFluentWorkflowItem<IFluentActivityItem>
    {
        /// <summary>
        /// Provide input for lamdba lambda function. Complex input type are serialized to JSON while for primitive types their string represenation is sent.
        /// </summary>
        /// <param name="input">Functor, which will return input for lambda function.</param>
        /// <returns></returns>
        IFluentLambdaItem WithInput(Func<ILambdaItem, object> input);

        /// <summary>
        /// Configure the timeout for lambda function. If lambda function does not finish within this period then SWF will raise LambdaFunctionTimedoutEvent.
        /// </summary>
        /// <param name="timout">Functor, which will return the timeout for lambda function</param>
        /// <returns></returns>
        IFluentLambdaItem WithTimeout(Func<ILambdaItem, TimeSpan?> timout);

        /// <summary>
        /// Provides the workflow action when lambda function is completed. By default on completion it will schedule it children as per Deflow algorithm.
        /// </summary>
        /// <param name="completedAction">Functtor, which will return the workflow action.</param>
        /// <returns></returns>
        IFluentLambdaItem OnCompletion(Func<LambdaCompletedEvent, WorkflowAction> completedAction);

        /// <summary>
        /// Provides the workflow action for lambda failed event. By default workflow is failed.
        /// </summary>
        /// <param name="failedAction"></param>
        /// <returns></returns>
        IFluentLambdaItem OnFailure(Func<LambdaFailedEvent, WorkflowAction> failedAction);

        /// <summary>
        /// Provides the workflow action for lambda timeout event. By default workflow is failed.
        /// </summary>
        /// <param name="timedoutAction"></param>
        /// <returns></returns>
        IFluentLambdaItem OnTimedout(Func<LambdaTimedoutEvent, WorkflowAction> timedoutAction);

        /// <summary>
        /// Provides the workflow action lamdba scheduling failed event. By default workflow is failed.
        /// </summary>
        /// <param name="schedulingFailedAction"></param>
        IFluentLambdaItem OnSchedulingFailed(Func<LambdaSchedulingFailedEvent, WorkflowAction> schedulingFailedAction);

        /// <summary>
        /// Provides the workflow action when SWF is failed to start the lamdba function. By default  workflow is failed.
        /// </summary>
        /// <param name="startFailedAction"></param>
        /// <returns></returns>
        IFluentLambdaItem OnStartFailed(Func<LambdaStartFailedEvent, WorkflowAction> startFailedAction);
    }
}