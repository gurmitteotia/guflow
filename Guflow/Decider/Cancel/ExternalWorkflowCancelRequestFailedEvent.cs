// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when SWF fails to deliver the workflow cancel request to external/child workflow.
    /// </summary>
    public class ExternalWorkflowCancelRequestFailedEvent :WorkflowItemEvent
    {
        private readonly RequestCancelExternalWorkflowExecutionFailedEventAttributes _eventAttributes;

        internal ExternalWorkflowCancelRequestFailedEvent(HistoryEvent cancelRequestFailedEvent):base(cancelRequestFailedEvent.EventId)
        {
            _eventAttributes = cancelRequestFailedEvent.RequestCancelExternalWorkflowExecutionFailedEventAttributes;
            ScheduleId = ScheduleId.Raw(_eventAttributes.WorkflowId);
        }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_SEND_CANCEL_REQUEST", Cause);
        }

        /// <summary>
        /// Return cause to give information on why cancel request to external workflow has failed.
        /// </summary>
        public string Cause => _eventAttributes.Cause;
    }
}