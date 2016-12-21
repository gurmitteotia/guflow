using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class WorkflowCompletionFailedEvent : WorkflowEvent
    {
        private readonly CompleteWorkflowExecutionFailedEventAttributes _eventAttributes;

        internal WorkflowCompletionFailedEvent(HistoryEvent workflowCompletionFailureEvent) :base(workflowCompletionFailureEvent.EventId)
        {
            _eventAttributes = workflowCompletionFailureEvent.CompleteWorkflowExecutionFailedEventAttributes;
        }

        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnWorkflowCompletionFailed(this);
        }
    }
}