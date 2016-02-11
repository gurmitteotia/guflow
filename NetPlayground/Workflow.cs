using System.Collections.Generic;

namespace NetPlayground
{
    public abstract class Workflow : IWorkflow
    {
        private readonly HashSet<SchedulableItem> _allSchedulableItems = new HashSet<SchedulableItem>();
 
        public WorkflowAction WorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            return new WorkflowStartedAction(workflowStartedEvent,_allSchedulableItems);
        }

        public WorkflowAction ActivityCompleted(ActivityCompletedEvent activityCompletedEvent)
        {
            var workflowActivity = _allSchedulableItems.FindActivity(activityCompletedEvent.Name, activityCompletedEvent.Version, activityCompletedEvent.PositionalName);
            
            if(workflowActivity==null)
                throw new IncompatibleWorkflowException(string.Format("Can not find activity by name {0}, version {1} and positional name {2} in workflow.",activityCompletedEvent.Name,activityCompletedEvent.Version, activityCompletedEvent.PositionalName));

            return workflowActivity.Completed(activityCompletedEvent);
        }

        public WorkflowAction ActivityFailed(ActivityFailedEvent activityFailedEvent)
        {
            var workflowActivity = _allSchedulableItems.FindActivity(activityFailedEvent.Name, activityFailedEvent.Version, activityFailedEvent.PositionalName);

            if (workflowActivity == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find activity by name {0}, version {1} and positional name {2} in workflow.", activityFailedEvent.Name, activityFailedEvent.Version, activityFailedEvent.PositionalName));

            return workflowActivity.Failed(activityFailedEvent);
        }

        protected ActivityItem AddActivity(string name, string version, string positionalName = "")
        {
            var runtimeActivity = new ActivityItem(name,version, positionalName,_allSchedulableItems);
            _allSchedulableItems.Add(runtimeActivity);
            return runtimeActivity;
        }
    }
}