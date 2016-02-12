using System;
using System.Collections.Generic;

namespace NetPlayground
{
    public class ActivityItem: SchedulableItem
    {
        private readonly HashSet<SchedulableItem> _allSchedulableItems;
        private Func<ActivityCompletedEvent, WorkflowAction> _onCompletionAction;
        private Func<ActivityFailedEvent, WorkflowAction> _onFailedAction; 
        public ActivityItem(string name, string version, string positionalName, HashSet<SchedulableItem> allSchedulableItems):base(name,version,positionalName)
        {
            _allSchedulableItems = allSchedulableItems;
            _onCompletionAction = c=>new ContinueWorkflowAction(this,c,_allSchedulableItems);
            _onFailedAction = c=> new FailWorkflowAction(c.Reason,c.Detail);
        }

        public ActivityItem DependsOn(string name, string version, string positionalName = "")
        {
            var parentItem = _allSchedulableItems.Find(name, version, positionalName);
            if(parentItem==null)
                throw new ParentItemNotFoundException(string.Format("Can not find the schedulable item by name {0}, version {1} and positional name {2}",name,version,positionalName));
            ParentItems.Add(parentItem);

            return this;
        }

        internal override WorkflowDecision GetDecision()
        {
            return new ScheduleActivityDecision(Name, Version, PositionalName);
        }

        internal WorkflowAction Completed(ActivityCompletedEvent activityCompletedEvent)
        {
            return _onCompletionAction(activityCompletedEvent);
        }

        protected override bool IsProcessed(IWorkflowContext workflowContext)
        {
            var activity = workflowContext.GetActivityEvent(Name, Version, PositionalName);

            return activity != null && activity.IsProcessed;
        }

        public ActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> onCompletionFunc)
        {
            _onCompletionAction = onCompletionFunc;
            return this;
        }

        public WorkflowAction Failed(ActivityFailedEvent activityFailedEvent)
        {
            return _onFailedAction(activityFailedEvent);
        }
    }
}