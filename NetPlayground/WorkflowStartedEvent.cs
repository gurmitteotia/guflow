using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class WorkflowStartedEvent : WorkflowEvent
    {
        public WorkflowStartedEvent(HistoryEvent workflowStartedEvent)
        {
            PopulateWorkflowStartedArgs(workflowStartedEvent.WorkflowExecutionStartedEventAttributes);
        }

        private WorkflowStartedArgs WorkflowStartedArgs { get; set; }
        

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowStarted(WorkflowStartedArgs);
        }

        private void PopulateWorkflowStartedArgs(WorkflowExecutionStartedEventAttributes startedAttributes)
        {
            WorkflowStartedArgs = new WorkflowStartedArgs();
            WorkflowStartedArgs.Input = startedAttributes.Input;
        }
    }
}
