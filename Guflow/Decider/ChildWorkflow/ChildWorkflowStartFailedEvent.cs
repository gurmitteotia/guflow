// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when child workflow is failed to start. By default workflow is failed on this event.
    /// </summary>
    public class ChildWorkflowStartFailedEvent : ChildWorkflowEvent
    {
        private readonly StartChildWorkflowExecutionFailedEventAttributes _attr;
        internal ChildWorkflowStartFailedEvent(HistoryEvent startFailedEvent, IEnumerable<HistoryEvent> allEvents)
            : base(startFailedEvent)
        {
            _attr = startFailedEvent.StartChildWorkflowExecutionFailedEventAttributes;
            PopulateProperties(string.Empty, _attr.InitiatedEventId, allEvents);
        }

        /// <summary>
        /// Returns cause for failure.
        /// </summary>
        public string Cause => _attr.Cause;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("CHILD_WORKFLOW_START_FAILED", Cause);
        }
    }
}