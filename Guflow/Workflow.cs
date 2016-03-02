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
            _onStartupFunc = s => WorkflowAction.StartWorkflow(this);
        }

        public WorkflowAction WorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            return _onStartupFunc(workflowStartedEvent);
        }

        public WorkflowAction ActivityCompleted(ActivityCompletedEvent activityCompletedEvent)
        {
            var workflowActivity = FindActivityFor(activityCompletedEvent);
            return workflowActivity.Completed(activityCompletedEvent);
        }

        public WorkflowAction ActivityFailed(ActivityFailedEvent activityFailedEvent)
        {
            var workflowActivity = FindActivityFor(activityFailedEvent);
            return workflowActivity.Failed(activityFailedEvent);
        }

        public WorkflowAction ActivityTimedout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            var workflowActivity = FindActivityFor(activityTimedoutEvent);
            return workflowActivity.Timedout(activityTimedoutEvent);
        }

        public WorkflowAction ActivityCancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            var workflowActivity = FindActivityFor(activityCancelledEvent);
            return workflowActivity.Cancelled(activityCancelledEvent);
        }

        public WorkflowAction TimerFired(TimerFiredEvent timerFiredEvent)
        {
            var workflowTimer = FindTimer(timerFiredEvent);
            if(workflowTimer!=null)
                return workflowTimer.Fired(timerFiredEvent);
            var rescheduleWorkflowItem = FindRescheduledItemFor(timerFiredEvent);
            return WorkflowAction.Schedule(rescheduleWorkflowItem);
        }

        public WorkflowAction TimerFailed(TimerFailedEvent timerFailedEvent)
        {
            return WorkflowAction.FailWorkflow("START_TIMER_FAILED", timerFailedEvent.Cause);
        }

        public WorkflowAction TimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            var workflowTimer = FindTimer(timerCancelledEvent);
            if (workflowTimer != null)
                return workflowTimer.TimerCancelled(timerCancelledEvent);
            var rescheduleWorkflowItem = FindRescheduledItemFor(timerCancelledEvent);
            return rescheduleWorkflowItem.TimerCancelled(timerCancelledEvent);
        }

        public WorkflowAction ActivityCancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            var workflowActivity = FindActivityFor(activityCancellationFailedEvent);
            return workflowActivity.CancellationFailed(activityCancellationFailedEvent);
        }

        public WorkflowAction TimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            var workflowTimer = FindTimerFor(timerCancellationFailedEvent);
            return workflowTimer.CancellationFailed(timerCancellationFailedEvent);
        }

        protected ActivityItem AddActivity(string name, string version, string positionalName = "")
        {
            var activityItem = new ActivityItem(name,version, positionalName,this);
            _allWorkflowItems.Add(activityItem);
            return activityItem;
        }

        protected TimerItem AddTimer(string name)
        {
            var timerItem = new TimerItem(name,this);
            _allWorkflowItems.Add(timerItem);

            return timerItem;
        }

        protected Workflow OnStartup(Func<WorkflowStartedEvent, WorkflowAction> workflowStartupFunc)
        {
            _onStartupFunc = workflowStartupFunc;
            return this;
        }

        protected WorkflowAction Continue(WorkflowItemEvent workflowEvent)
        {
            var workfowItem = FindWorkflowItemFor(workflowEvent);

            return new ContinueWorkflowAction(workfowItem,workflowEvent.WorkflowContext);
        }

        protected WorkflowAction FailWorkflow(string reason, string detail)
        {
            return WorkflowAction.FailWorkflow(reason, detail);
        }

        protected WorkflowAction CompleteWorkflow(string result)
        {
            return WorkflowAction.CompleteWorkflow(result);
        }

        protected WorkflowAction CancelWorkflow(string details)
        {
            return WorkflowAction.CancelWorkflow(details);
        }
        protected ScheduleWorkflowItemAction Reschedule(WorkflowItemEvent workflowItemEvent)
        {
            var workflowItem = FindWorkflowItemFor(workflowItemEvent);
            return WorkflowAction.Schedule(workflowItem);
        }
        protected WorkflowAction StartWorkflow()
        {
            return WorkflowAction.StartWorkflow(this);
        }
        protected WorkflowAction Ignore()
        {
            return WorkflowAction.Ignore;
        }
        protected ScheduleWorkflowItemAction ScheduleActivity(string name, string version, string position)
        {
            var activityItem = FindActivityFor(Identity.New(name,version,position));
            return WorkflowAction.Schedule(activityItem);
        }
        protected WorkflowAction ScheduleTimer(string name)
        {
            var activityItem = FindTimerFor(Identity.Timer(name));
            return WorkflowAction.Schedule(activityItem);
        }
        protected WorkflowAction CancelActivity(string name, string version, string position)
        {
            var activityItem = FindActivityFor(Identity.New(name, version, position));
            return WorkflowAction.Cancel(activityItem);
        }
        protected WorkflowAction CancelTimer(string name)
        {
            var activityItem = FindTimerFor(Identity.Timer(name));
            return WorkflowAction.Cancel(activityItem);
        }
        public IEnumerable<WorkflowItem> GetStartupWorkflowItems()
        {
            return _allWorkflowItems.Where(s => s.HasNoParents());
        }

        public  IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem item)
        {
            return _allWorkflowItems.Where(s => s.IsChildOf(item));
        }

        public WorkflowItem Find(string name, string version, string positionalName)
        {
            return Find(Identity.New(name, version, positionalName));
        }

        public WorkflowItem Find(Identity identity)
        {
            return _allWorkflowItems.FirstOrDefault(s => s.Has(identity));
        }

        public ActivityItem FindActivity(string name, string version, string positionalName)
        {
            return FindActivity(Identity.New(name, version, positionalName));
        }

        public ActivityItem FindActivity(Identity identity)
        {
            return _allWorkflowItems.OfType<ActivityItem>().FirstOrDefault(a => a.Has(identity));
        }

        private ActivityItem FindActivity(WorkflowItemEvent activityEvent)
        {
            return _allWorkflowItems.OfType<ActivityItem>().FirstOrDefault(activityEvent.IsFor);
        }
        private TimerItem FindTimer(WorkflowItemEvent activityEvent)
        {
            return _allWorkflowItems.OfType<TimerItem>().FirstOrDefault(activityEvent.IsFor);
        }
        public TimerItem FindTimer(Identity identity)
        {
            return _allWorkflowItems.OfType<TimerItem>().FirstOrDefault(s => s.Has(identity));
        }

        private TimerItem FindTimer(TimerEvent timerFiredEvent)
        {
            return _allWorkflowItems.OfType<TimerItem>().FirstOrDefault(timerFiredEvent.IsFor);
        }

        private ActivityItem FindActivityFor(ActivityEvent activityEvent)
        {
            var workflowActivity = FindActivity(activityEvent);

            if (workflowActivity == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find activity by name {0}, version {1} and positional name {2} in workflow.", activityEvent.Name, activityEvent.Version, activityEvent.PositionalName));

            return workflowActivity;
        }
        private ActivityItem FindActivityFor(WorkflowItemEvent workflowItemEvent)
        {
            var workflowActivity = FindActivity(workflowItemEvent);

            if (workflowActivity == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find activity for event {0}.", workflowItemEvent ));

            return workflowActivity;
        }
        private TimerItem FindTimerFor(WorkflowItemEvent workflowItemEvent)
        {
            var workflowTimer = FindTimer(workflowItemEvent);
            if (workflowTimer == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find timer for event {0}.", workflowItemEvent));
            return workflowTimer;
        }
        private ActivityItem FindActivityFor(Identity identity)
        {
            var workflowActivity = FindActivity(identity);

            if (workflowActivity == null)
                throw new WorkflowItemNotFoundException(string.Format("Can not find activity by name {0}, version {1} and positional name {2} in workflow.", identity.Name, identity.Version, identity.PositionalName));
            return workflowActivity;
        }
        private TimerItem FindTimerFor(Identity identity)
        {
            var workflowTimer = FindTimer(identity);
            if (workflowTimer == null)
                throw new WorkflowItemNotFoundException(string.Format("Can not find timer by name {0}.", identity.Name));
            return workflowTimer;
        }
        private WorkflowItem FindRescheduledItemFor(TimerEvent timerFiredEvent)
        {
            var workflowItem = _allWorkflowItems.FirstOrDefault(timerFiredEvent.IsRescheduleTimerFor);

            if (workflowItem == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find timer by name {0} in workflow.", timerFiredEvent.Name));

            return workflowItem;
        }

        private WorkflowItem FindWorkflowItemFor(WorkflowItemEvent workflowItemEvent)
        {
            var workflowItem = _allWorkflowItems.FirstOrDefault(workflowItemEvent.IsFor);

            if (workflowItem == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find workflow item for event {0}", workflowItemEvent));

            return workflowItem;
        }
    }
}