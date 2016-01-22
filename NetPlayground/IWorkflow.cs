namespace NetPlayground
{
    public interface IWorkflow
    {
        WorkflowAction WorkflowStarted(WorkflowStartedArgs workflowStartedArgs);
        WorkflowAction ActivityCompleted(ActivityCompletedEvent activityCompletedEvent);
        WorkflowAction ActivityFailed(ActivityFailedEvent activityFailedEvent);
    }
}