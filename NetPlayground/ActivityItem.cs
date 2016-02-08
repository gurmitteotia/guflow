using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ActivityItem: SchedulableItem
    {
        private readonly HashSet<SchedulableItem> _allSchedulableItems;
        private Func<ActivityCompletedEvent, WorkflowAction> _onCompletionAction;
 
        public ActivityItem(string name, string version, string positionalName, HashSet<SchedulableItem> allSchedulableItems):base(name,version,positionalName)
        {
            _allSchedulableItems = allSchedulableItems;
            _onCompletionAction = c=>new ContinueWorkflowAction(this,c,_allSchedulableItems);
        }

        public ActivityItem DependsOn(string name, string version, string positionalName = "")
        {
            var parentItem = _allSchedulableItems.Find(name, version, positionalName);
            if(parentItem==null)
                throw new ParentItemNotFoundException(string.Format("Can not find the schedulable item by name {0}, version {1} and positional name {2}",name,version,positionalName));
            ParentItems.Add(parentItem);

            return this;
        }

        internal override Decision GetDecision()
        {
            return new Decision()
            {
                ScheduleActivityTaskDecisionAttributes = new ScheduleActivityTaskDecisionAttributes()
                {
                    ActivityType = new ActivityType() {Name = Name, Version = Version},
                },
                DecisionType = DecisionType.ScheduleActivityTask
            };
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
    }
}