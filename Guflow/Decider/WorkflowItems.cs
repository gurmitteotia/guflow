using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class WorkflowItems
    {
        private readonly HashSet<WorkflowItem> _workflowItems = new HashSet<WorkflowItem>();

        public bool Add(WorkflowItem workflowItem)
        {
            return _workflowItems.Add(workflowItem);
        }
        public ActivityItem FindActivityFor(WorkflowItemEvent workflowItemEvent)
        {
            var workflowActivity = ActivityOf(workflowItemEvent);

            if (workflowActivity == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find activity for event {0}.", workflowItemEvent));

            return workflowActivity;
        }

        public ActivityItem FindActivityFor(Identity identity)
        {
            var workflowActivity = ActivityOf(identity);

            if (workflowActivity == null)
                throw new WorkflowItemNotFoundException(string.Format("Can not find activity by name {0}, version {1} and positional markerName {2} in workflow.", identity.Name, identity.Version, identity.PositionalName));
            return workflowActivity;
        }
        public TimerItem FindTimerFor(Identity identity)
        {
            var workflowTimer = TimerOf(identity);
            if (workflowTimer == null)
                throw new WorkflowItemNotFoundException(string.Format("Can not find timer by name {0}.", identity.Name));
            return workflowTimer;
        }
        public TimerItem FindTimerFor(WorkflowItemEvent workflowItemEvent)
        {
            var workflowItem = TimerOf(workflowItemEvent);

            if (workflowItem == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find workflow item for event {0}", workflowItemEvent));

            return workflowItem;
        }

        public IEnumerable<WorkflowItem> StartupItems()
        {
            return _workflowItems.Where(i => i.HasNoParents());
        }

        public IEnumerable<WorkflowItem> ChilderenOf(WorkflowItem workflowItem)
        {
            return _workflowItems.Where(i => i.IsChildOf(workflowItem));
        }
        public WorkflowItem FindWorkflowItemFor(WorkflowItemEvent workflowItemEvent)
        {
            var workflowItem = _workflowItems.FirstOrDefault(workflowItemEvent.IsFor);

            if (workflowItem == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find workflow item for event {0}", workflowItemEvent));

            return workflowItem;
        }

        public WorkflowItem WorkflowItemOf(Identity identity)
        {
            return _workflowItems.FirstOrDefault(i => i.Has(identity));
        }
        public ActivityItem ActivityOf(WorkflowItemEvent activityEvent)
        {
            return _workflowItems.OfType<ActivityItem>().FirstOrDefault(activityEvent.IsFor);
        }

        private TimerItem TimerOf(Identity identity)
        {
            return _workflowItems.OfType<TimerItem>().FirstOrDefault(s => s.Has(identity));
        }
        public TimerItem TimerOf(WorkflowItemEvent timerEvent)
        {
            return _workflowItems.OfType<TimerItem>().FirstOrDefault(timerEvent.IsFor);
        }

        public IEnumerable<IWorkflowItem> AllItems
        {
            get { return _workflowItems; }
        }

        public IEnumerable<IActivityItem> AllActivities
        {
            get { return _workflowItems.OfType<ActivityItem>(); }
        }

        public IEnumerable<ITimerItem> AllTimers
        {
            get { return _workflowItems.OfType<TimerItem>(); }
        }
        private ActivityItem ActivityOf(Identity identity)
        {
            return _workflowItems.OfType<ActivityItem>().FirstOrDefault(a => a.Has(identity));
        }
    }
}