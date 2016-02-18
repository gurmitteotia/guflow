using System;
using System.Collections.Generic;

namespace Guflow
{
    public class ActivityItem: WorkflowItem
    {
        private readonly IWorkflowItems _workflowItems;
        private Func<ActivityCompletedEvent, WorkflowAction> _onCompletionAction;
        private Func<ActivityFailedEvent, WorkflowAction> _onFailedAction;
        private Func<ActivityTimedoutEvent, WorkflowAction> _onTimedoutAction;
        private Func<ActivityCancelledEvent, WorkflowAction> _onCancelledAction;
 
        public ActivityItem(string name, string version, string positionalName, IWorkflowItems workflowItems):base(name,version,positionalName)
        {
            _workflowItems = workflowItems;
            _onCompletionAction = c => new ContinueWorkflowAction(this, c.WorkflowContext);
            _onFailedAction = c=> new FailWorkflowAction(c.Reason,c.Detail);
            _onTimedoutAction = t=>new FailWorkflowAction(t.TimeoutType,t.Details);
            _onCancelledAction = c=>new CancelWorkflowAction(c.Details);
        }

        public ActivityItem DependsOn(string name, string version, string positionalName = "")
        {
            var parentItem = _workflowItems.Find(name, version, positionalName);
            if(parentItem==null)
                throw new ParentItemNotFoundException(string.Format("Can not find the schedulable item by name {0}, version {1} and positional name {2}",name,version,positionalName));
            ParentItems.Add(parentItem);

            return this;
        }

        internal override IEnumerable<WorkflowItem> GetChildlern()
        {
            return _workflowItems.GetChildernOf(this);
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

            return activity != null;
        }

        public ActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> onCompletionFunc)
        {
            _onCompletionAction = onCompletionFunc;
            return this;
        }

        internal WorkflowAction Failed(ActivityFailedEvent activityFailedEvent)
        {
            return _onFailedAction(activityFailedEvent);
        }

        public ActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> onFailureFunc)
        {
            _onFailedAction = onFailureFunc;
            return this;
        }

        internal WorkflowAction Timedout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            return _onTimedoutAction(activityTimedoutEvent);
        }

        public ActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> onTimedoutFunc)
        {
            _onTimedoutAction = onTimedoutFunc;
            return this;
        }

        public WorkflowAction Cancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            return _onCancelledAction(activityCancelledEvent);
        }

        public ActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> onCancelledFunc)
        {
            _onCancelledAction = onCancelledFunc;
            return this;
        }
    }
}