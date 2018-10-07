// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when child workflow is failed.
    /// </summary>
    public class ChildWorkflowFailedEvent : ChildWorkflowEvent
    {
        private readonly ChildWorkflowExecutionFailedEventAttributes _attr;
        internal ChildWorkflowFailedEvent(HistoryEvent failedEvent, IEnumerable<HistoryEvent> allEvents) : base(failedEvent.EventId)
        {
            _attr = failedEvent.ChildWorkflowExecutionFailedEventAttributes;
            PopulateProperties(_attr.WorkflowExecution.RunId, _attr.InitiatedEventId, allEvents);
        }

        /// <summary>
        /// Returns reason on why child workflow is failed.
        /// </summary>
        public string Reason => _attr.Reason;

        /// <summary>
        /// Returns details on about failure.
        /// </summary>
        public string Details => _attr.Details;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow(Reason, Details);
        }
    }
}