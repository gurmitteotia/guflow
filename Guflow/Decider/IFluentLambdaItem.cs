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
        /// Provide input to scheduling lambda function.
        /// </summary>
        /// <param name="input">Functor, which will return input for lambda function.</param>
        /// <returns></returns>
        IFluentLambdaItem WithInput(Func<ILambdaItem, object> input);

        /// <summary>
        /// Provides the timeout for lambda function to finish. If lambda function does not finish within this period then SWF will raise LambdaFunctionTimedoutEvent.
        /// </summary>
        /// <param name="timout">Functor, which will return the timeout for lambda function</param>
        /// <returns></returns>
        IFluentLambdaItem WithTimeout(Func<ILambdaItem, TimeSpan?> timout);
    }
}