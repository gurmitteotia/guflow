using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class WorkflowCancelRequestFailedEvent :WorkflowEvent
    {
        private readonly RequestCancelExternalWorkflowExecutionFailedEventAttributes _eventAttributes;

        internal WorkflowCancelRequestFailedEvent(HistoryEvent cancelRequestFailedEvent):base(cancelRequestFailedEvent.EventId)
        {
            _eventAttributes = cancelRequestFailedEvent.RequestCancelExternalWorkflowExecutionFailedEventAttributes;
        }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnWorkflowCancelRequestFailed(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_SEND_CANCEL_REQUEST", Cause);
        }

        public string Cause {get { return _eventAttributes.Cause; }}
    }
}