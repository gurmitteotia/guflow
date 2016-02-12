using System.Collections.Generic;

namespace NetPlayground
{
    public interface IWorkflow
    {
        WorkflowAction WorkflowStarted(WorkflowStartedEvent workflowStartedEvent);
        WorkflowAction ActivityCompleted(ActivityCompletedEvent activityCompletedEvent);
        WorkflowAction ActivityFailed(ActivityFailedEvent activityFailedEvent);
    }
}