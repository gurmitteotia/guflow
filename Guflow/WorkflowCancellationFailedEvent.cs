using Amazon.SimpleWorkflow.Model;

namespace Guflow
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
    }
}