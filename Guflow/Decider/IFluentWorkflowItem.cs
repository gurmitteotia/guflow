// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Worker;

namespace Guflow.Decider
{
    public interface IFluentWorkflowItem<out T>
    {
        /// <summary>
        /// Schedule this item after the named timer is fired.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        T AfterTimer(string name);
        /// <summary>
        /// Schedule this item after named activity is completed.
        /// </summary>
        /// <param name="name">Parent activity name.</param>
        /// <param name="version">Parent activity version</param>
        /// <param name="positionalName">Parent activity positional name.</param>
        /// <returns></returns>
        T AfterActivity(string name, string version, string positionalName = "");
        /// <summary>
        /// Schedule this item after TActivity is compeleted.
        /// </summary>
        /// <typeparam name="TActivity">Activity type.</typeparam>
        /// <param name="positionalName">Positional name.</param>
        /// <returns></returns>
        T AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity;

        /// <summary>
        /// Schedule this item after named lambda function is completed.
        /// </summary>
        /// <param name="name">Lambda function name</param>
        /// <param name="positionalName">Positional name</param>
        /// <returns></returns>
        T AfterLambda(string name, string positionalName = "");

        /// <summary>
        /// Schedule this item after the child workflow.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        T AfterChildWorkflow(string name, string version, string positionalName = "");

        /// <summary>
        /// Schedule the item after child workflow.
        /// </summary>
        /// <typeparam name="TWorkflow">Workflow type.</typeparam>
        /// <param name="positionalName">Positional name, if any</param>
        /// <returns></returns>
        T AfterChildWorkflow<TWorkflow>(string positionalName ="") where TWorkflow : Workflow;
    }
}