// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

namespace Guflow.Decider
{
    /// <summary>
    /// Represent a child workflow item.
    /// </summary>
    public interface IChildWorkflowItem : IWorkflowItem
    {
        /// <summary>
        /// Returns name of child workflow.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns version of child workflow.
        /// </summary>
        string Version { get; }


        /// <summary>
        /// Returns positional name of child workflow.
        /// </summary>
        string PositionalName { get; }
    }
}