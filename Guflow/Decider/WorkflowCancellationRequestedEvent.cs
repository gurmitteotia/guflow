using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class WorkflowCancellationRequestedEvent : WorkflowEvent
    {
        private readonly WorkflowExecutionCancelRequestedEventAttributes _eventAttributes;

        internal WorkflowCancellationRequestedEvent(HistoryEvent cancellationRequestedEvent)
            : base(cancellationRequestedEvent.EventId)
        {
            _eventAttributes = cancellationRequestedEvent.WorkflowExecutionCancelRequestedEventAttributes;
        }

        public string Cause { get { return _eventAttributes.Cause; } }

        public string ExternalWorkflowRunid
        {
            get
            {
                return _eventAttributes.ExternalWorkflowExecution != null
                    ? _eventAttributes.ExternalWorkflowExecution.RunId
                    : null;
            }
        }
        public string ExternalWorkflowId
        {
            get
            {
                return _eventAttributes.ExternalWorkflowExecution != null
                    ? _eventAttributes.ExternalWorkflowExecution.WorkflowId
                    : null;
            }
        }
        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnWorkflowCancellationRequested(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.CancelWorkflow(Cause);
        }
    }
}