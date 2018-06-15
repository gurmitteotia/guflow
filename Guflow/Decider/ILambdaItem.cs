// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.
namespace Guflow.Decider
{
    /// <summary>
    /// Represents the scheduled lambda by workflow.
    /// </summary>
    public interface ILambdaItem : IWorkflowItem
    {
        /// <summary>
        /// Returns name of lambda.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns positional name of lambda function. Positional name acts as differentiator when scheduling same lambda function multiple times in workflow.
        /// </summary>
        string PositionalName { get; }
    }
}