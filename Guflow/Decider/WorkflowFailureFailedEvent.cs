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

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnWorkflowFailureFailed(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_FAIL_WORKFLOW", Cause);
        }
    }
}