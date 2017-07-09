namespace Guflow.Decider
{
    internal interface IWorkflowDefaultActions
    {
        WorkflowAction Continue(WorkflowItemEvent workflowItemEvent);
        WorkflowAction StartWorkflow();
        WorkflowAction FailWorkflow(string reason, string details);
        WorkflowAction CancelWorkflow(string details);
        WorkflowAction Ignore();
    }
}