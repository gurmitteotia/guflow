// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when child workflow is cancelled.
    /// </summary>
    public sealed class ChildWorkflowCancelledEvent : ChildWorkflowEvent
    {
        private ChildWorkflowExecutionCanceledEventAttributes _attr;
        internal ChildWorkflowCancelledEvent(HistoryEvent cancelledEvent, IEnumerable<HistoryEvent> allEvents)
            : base(cancelledEvent.EventId)
        {
            _attr = cancelledEvent.ChildWorkflowExecutionCanceledEventAttributes;
            PopulateProperties(_attr.WorkflowExecution.RunId, _attr.InitiatedEventId, allEvents);
        }

        /// <summary>
        /// Returns cancellation details.
        /// </summary>
        public string Details => _attr.Details;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.CancelWorkflow(Details);
        }
    }
}