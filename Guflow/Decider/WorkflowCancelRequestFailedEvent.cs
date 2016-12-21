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

        public string Cause {get { return _eventAttributes.Cause; }}
    }
}