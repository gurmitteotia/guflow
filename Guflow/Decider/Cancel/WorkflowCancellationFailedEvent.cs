// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when SWF fails to cancel this workflow.
    /// </summary>
    public class WorkflowCancellationFailedEvent : WorkflowEvent
    {
        private readonly CancelWorkflowExecutionFailedEventAttributes _eventAttributes;
        internal WorkflowCancellationFailedEvent(HistoryEvent cancellationFailedEvent)
            : base(cancellationFailedEvent)
        {
            _eventAttributes = cancellationFailedEvent.CancelWorkflowExecutionFailedEventAttributes;
        }

        /// <summary>
        /// Gets the reason on why workflow could not be cancelled.
        /// </summary>
        public string Cause => _eventAttributes.Cause;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_CANCEL_WORKFLOW", Cause);
        }
    }
}