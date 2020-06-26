// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when child workflow is timedout.
    /// </summary>
    public sealed class ChildWorkflowTimedoutEvent : ChildWorkflowEvent
    {
        private readonly ChildWorkflowExecutionTimedOutEventAttributes _attr;

        internal ChildWorkflowTimedoutEvent(HistoryEvent timedoutEvent, IEnumerable<HistoryEvent> allEvents)
            : base(timedoutEvent)
        {
            _attr = timedoutEvent.ChildWorkflowExecutionTimedOutEventAttributes;
            PopulateProperties(_attr.WorkflowExecution.RunId, _attr.InitiatedEventId, allEvents);
        }

        /// <summary>
        /// Returns the reason for timout.
        /// </summary>
        public string TimedoutType => _attr.TimeoutType;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("CHILD_WORKFLOW_TIMEDOUT", TimedoutType);
        }
    }
}