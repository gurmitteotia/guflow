using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class WorkflowStartedEvent : WorkflowEvent
    {
        private WorkflowExecutionStartedEventAttributes _workflowStartedAttributes;

        public WorkflowStartedEvent(HistoryEvent workflowStartedEvent)
        {
            _workflowStartedAttributes = workflowStartedEvent.WorkflowExecutionStartedEventAttributes;
        }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowStarted(this);
        }
    }
}
