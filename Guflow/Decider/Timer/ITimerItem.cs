// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent the timer to schedule in a workflow.
    /// </summary>
    public interface ITimerItem : IWorkflowItem
    {
        /// <summary>
        /// Returns name of timer.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Cancel the already scheduled timer and schedule it again with last timeout. Throws exception if timer is not already active.
        /// </summary>
        /// <returns></returns>
        WorkflowAction Reset();
        /// <summary>
        /// Cancel the already scheduled timer and schedule it again with new timeout. Throws exception if timer is not already active.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        WorkflowAction Reschedule(TimeSpan timeout);
    }
}