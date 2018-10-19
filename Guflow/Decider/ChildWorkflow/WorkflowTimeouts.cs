// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent timeouts for the child workflow.
    /// </summary>
    public struct WorkflowTimeouts
    {
        /// <summary>
        /// Indicates how long workflow can live in Amazon SWF. If it is not completed by this time then it is timedout.
        /// </summary>
        public TimeSpan? ExecutionStartToCloseTimeout { get; set; }

        /// <summary>
        /// Indicate how quickly decider should return the decisions to Amazon SWF.
        /// </summary>
        public TimeSpan? TaskStartToCloseTimeout { get; set; }
    }
}