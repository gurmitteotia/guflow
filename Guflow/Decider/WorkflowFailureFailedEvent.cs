using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class WorkflowFailureFailedEvent: WorkflowEvent
    {
        private readonly FailWorkflowExecutionFailedEventAttributes _eventAttributes;
        internal WorkflowFailureFailedEvent(HistoryEvent workflowFailureFailedEvent) :base(workflowFailureFailedEvent.EventId)
        {
            _eventAttributes = workflowFailureFailedEvent.FailWorkflowExecutionFailedEventAttributes;
        }
        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnWorkflowFailureFailed(this);
        }
    }
}