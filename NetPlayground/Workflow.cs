using System.Collections.Generic;

namespace NetPlayground
{
    public abstract class Workflow : IWorkflow
    {
        private readonly HashSet<SchedulableItem> _workflowSchedulableItems = new HashSet<SchedulableItem>();
 
        public WorkflowAction WorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            var startupSchedulableItem = _workflowSchedulableItems.GetStartupItems();

            return new WorkflowStartedAction(startupSchedulableItem);
        }

        public WorkflowAction ActivityCompleted(ActivityCompletedEvent activityCompletedEvent)
        {
            var childItems = _workflowSchedulableItems.GetChildernOf(activityCompletedEvent.ActivityName);
            return new ScheduleItemsAction(childItems);
        }

        public WorkflowAction ActivityFailed(ActivityFailedEvent activityFailedEvent)
        {
            throw new System.NotImplementedException();
        }

        protected ActivityItem AddActivity(string name, string version)
        {
            var runtimeActivity = new ActivityItem(name,version);
            _workflowSchedulableItems.Add(runtimeActivity);
            return runtimeActivity;
        }
    }
}