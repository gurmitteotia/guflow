using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public abstract class Workflow : IWorkflow, IWorkflowItems
    {
        private readonly HashSet<WorkflowItem> _allWorkflowItems = new HashSet<WorkflowItem>();
        private Func<WorkflowStartedEvent, WorkflowAction> _onStartupFunc;

        protected Workflow()
        {
            _onStartupFunc = s => new WorkflowStartedAction(s, this);
        }
        

        public WorkflowAction WorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            return _onStartupFunc(workflowStartedEvent);
        }

        public WorkflowAction ActivityCompleted(ActivityCompletedEvent activityCompletedEvent)
        {
            var workflowActivity = ActivityFor(activityCompletedEvent);
            return workflowActivity.Completed(activityCompletedEvent);
        }

        public WorkflowAction ActivityFailed(ActivityFailedEvent activityFailedEvent)
        {
            var workflowActivity = ActivityFor(activityFailedEvent);
            return workflowActivity.Failed(activityFailedEvent);
        }

        public WorkflowAction ActivityTimedout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            var workflowActivity = ActivityFor(activityTimedoutEvent);
            return workflowActivity.Timedout(activityTimedoutEvent);
        }

        public WorkflowAction ActivityCancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            var workflowActivity = ActivityFor(activityCancelledEvent);
            return workflowActivity.Cancelled(activityCancelledEvent);
        }

        protected ActivityItem AddActivity(string name, string version, string positionalName = "")
        {
            var runtimeActivity = new ActivityItem(name,version, positionalName,this);
            _allWorkflowItems.Add(runtimeActivity);
            return runtimeActivity;
        }

        protected Workflow OnStartup(Func<WorkflowStartedEvent, WorkflowAction> workflowStartupFunc)
        {
            _onStartupFunc = workflowStartupFunc;
            return this;
        }

        public IEnumerable<WorkflowItem> GetStartupItems()
        {
            return _allWorkflowItems.Where(s => s.HasNoParents());
        }

        public  IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem item)
        {
            return _allWorkflowItems.Where(s => s.IsChildOf(item));
        }

        public WorkflowItem Find(string name, string version, string positionalName)
        {
            return _allWorkflowItems.FirstOrDefault(s => s.Has(name, version, positionalName));
        }

        public ActivityItem FindActivity(string name, string version, string positionalName)
        {
            return _allWorkflowItems.OfType<ActivityItem>().FirstOrDefault(s => s.Has(name, version, positionalName));
        }

        private ActivityItem ActivityFor(ActivityEvent activityEvent)
        {
            var workflowActivity = FindActivity(activityEvent.Name, activityEvent.Version, activityEvent.PositionalName);

            if (workflowActivity == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find activity by name {0}, version {1} and positional name {2} in workflow.", activityEvent.Name, activityEvent.Version, activityEvent.PositionalName));

            return workflowActivity;
        }
    }
}