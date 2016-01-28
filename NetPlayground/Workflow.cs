using System.Collections.Generic;

namespace NetPlayground
{
    public abstract class Workflow : IWorkflow
    {
        private readonly HashSet<SchedulableItem> _allSchedulableItems = new HashSet<SchedulableItem>();
 
        public WorkflowAction WorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            var startupSchedulableItem = _allSchedulableItems.GetStartupItems();

            return new WorkflowStartedAction(startupSchedulableItem);
        }

        public WorkflowAction ActivityCompleted(ActivityCompletedEvent activityCompletedEvent)
        {
            var completedSchedulableItem = _allSchedulableItems.Find(activityCompletedEvent.Name, activityCompletedEvent.Version, activityCompletedEvent.PositionalName);
            
            if(completedSchedulableItem==null)
                throw new IncompatibleWorkflowException(string.Format("Can not find activity by name {0}, version {1} and positional name {2} in workflow.",activityCompletedEvent.Name,activityCompletedEvent.Version, activityCompletedEvent.PositionalName));

            var childItems = _allSchedulableItems.GetChildernOf(completedSchedulableItem);
            return new ScheduleItemsAction(childItems);
        }

        public WorkflowAction ActivityFailed(ActivityFailedEvent activityFailedEvent)
        {
            throw new System.NotImplementedException();
        }

        protected ActivityItem AddActivity(string name, string version, string positionalName = "")
        {
            var runtimeActivity = new ActivityItem(name,version, positionalName,_allSchedulableItems);
            _allSchedulableItems.Add(runtimeActivity);
            return runtimeActivity;
        }
    }
}