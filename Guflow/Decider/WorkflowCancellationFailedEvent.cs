using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class WorkflowCancellationFailedEvent : WorkflowEvent
    {
        private readonly CancelWorkflowExecutionFailedEventAttributes _eventAttributes;
        internal WorkflowCancellationFailedEvent(HistoryEvent cancellationFailedEvent)
            : base(cancellationFailedEvent.EventId)
        {
            _eventAttributes = cancellationFailedEvent.CancelWorkflowExecutionFailedEventAttributes;
        }

        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnWorkflowCancellationFailed(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_CANCEL_WORKFLOW", Cause);
        }
    }
}