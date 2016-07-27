using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow
{
    public abstract class Workflow : IWorkflow, IWorkflowClosingActions
    {
        private readonly HashSet<WorkflowItem> _allWorkflowItems = new HashSet<WorkflowItem>();
        private IWorkflowHistoryEvents _currentworkflowHistoryEvents;
        WorkflowAction IWorkflowActions.OnWorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            return OnStart(workflowStartedEvent);
        }

        WorkflowAction IWorkflowActions.OnActivityCompletion(ActivityCompletedEvent activityCompletedEvent)
        {
            var workflowActivity = FindActivityFor(activityCompletedEvent);
            return workflowActivity.Completed(activityCompletedEvent);
        }
        WorkflowAction IWorkflowActions.OnActivityFailure(ActivityFailedEvent activityFailedEvent)
        {
            var workflowActivity = FindActivityFor(activityFailedEvent);
            return workflowActivity.Failed(activityFailedEvent);
        }
        WorkflowAction IWorkflowActions.OnActivityTimeout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            var workflowActivity = FindActivityFor(activityTimedoutEvent);
            return workflowActivity.Timedout(activityTimedoutEvent);
        }
        WorkflowAction IWorkflowActions.OnActivityCancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            var workflowActivity = FindActivityFor(activityCancelledEvent);
            return workflowActivity.Cancelled(activityCancelledEvent);
        }
        WorkflowAction IWorkflowActions.OnTimerFired(TimerFiredEvent timerFiredEvent)
        {
            var workflowItem = FindWorkflowItemFor(timerFiredEvent);
            return workflowItem.TimerFired(timerFiredEvent);
        }
        WorkflowAction IWorkflowActions.OnTimerStartFailure(TimerStartFailedEvent timerStartFailedEvent)
        {
            var workflowItem = FindWorkflowItemFor(timerStartFailedEvent);
            return workflowItem.TimerStartFailed(timerStartFailedEvent);
        }
        WorkflowAction IWorkflowActions.OnTimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            var workflowItem = FindWorkflowItemFor(timerCancelledEvent);
            return workflowItem.TimerCancelled(timerCancelledEvent);
        }
        WorkflowAction IWorkflowActions.OnActivityCancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            var workflowActivity = FindActivityFor(activityCancellationFailedEvent);
            return workflowActivity.CancellationFailed(activityCancellationFailedEvent);
        }
        WorkflowAction IWorkflowActions.OnTimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            var workflowItem = FindWorkflowItemFor(timerCancellationFailedEvent);
            return workflowItem.TimerCancellationFailed(timerCancellationFailedEvent);
        }
        WorkflowAction IWorkflowActions.OnActivitySchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent)
        {
            var workflowActivity = FindActivityFor(activitySchedulingFailedEvent);
            return workflowActivity.SchedulingFailed(activitySchedulingFailedEvent);
        }
        WorkflowAction IWorkflowActions.OnWorkflowSignaled(WorkflowSignaledEvent workflowSignaledEvent)
        {
            return OnSignal(workflowSignaledEvent);
        }
        WorkflowAction IWorkflowActions.OnWorkflowCancellationRequested(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent)
        {
            return OnCancellationRequested(workflowCancellationRequestedEvent);
        }

        protected IFluentActivityItem ScheduleActivity(string name, string version, string positionalName = "")
        {
            var activityItem = new ActivityItem(Identity.New(name,version, positionalName),this);
            if(!_allWorkflowItems.Add(activityItem))
                throw new DuplicateItemException(string.Format(Resources.Duplicate_activity,name,version,positionalName));
            return activityItem;
        }
        protected IFluentTimerItem ScheduleTimer(string name)
        {
            var timerItem = new TimerItem(Identity.Timer(name),this);
            if(!_allWorkflowItems.Add(timerItem))
                throw new DuplicateItemException(string.Format(Resources.Duplicate_timer,name));

            return timerItem;
        }
        protected virtual WorkflowAction OnStart(WorkflowStartedEvent workflowSartedEvent)
        {
            return WorkflowAction.StartWorkflow(this);
        }
        protected virtual WorkflowAction OnSignal(WorkflowSignaledEvent workflowSignaledEvent)
        {
            return WorkflowAction.Ignore;
        }
        protected virtual WorkflowAction OnCancellationRequested(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent)
        {
            return WorkflowAction.CancelWorkflow(workflowCancellationRequestedEvent.Cause);
        }
        protected WorkflowAction Continue(WorkflowItemEvent workflowEvent)
        {
            var workfowItem = FindWorkflowItemFor(workflowEvent);
            return WorkflowAction.ContinueWorkflow(workfowItem);
        }
        protected WorkflowAction FailWorkflow(string reason, string details)
        {
            return WorkflowAction.FailWorkflow(reason, details);
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
        protected ScheduleWorkflowItemAction JumpToActivity(string name, string version, string positionalName="")
        {
            var activityItem = FindActivityFor(Identity.New(name,version,positionalName));
            return WorkflowAction.Schedule(activityItem);
        }
        protected WorkflowAction JumpToTimer(string name)
        {
            var activityItem = FindTimerFor(Identity.Timer(name));
            return WorkflowAction.Schedule(activityItem);
        }
        protected WorkflowAction CancelActivity(string name, string version, string positionalName="")
        {
            var activityItem = FindActivityFor(Identity.New(name, version, positionalName));
            return WorkflowAction.Cancel(activityItem);
        }
        protected WorkflowAction CancelTimer(string name)
        {
            var activityItem = FindTimerFor(Identity.Timer(name));
            return WorkflowAction.Cancel(activityItem);
        }
        protected IActivityItem ActivityOf(WorkflowItemEvent activityEvent)
        {
            return _allWorkflowItems.OfType<ActivityItem>().FirstOrDefault(activityEvent.IsFor);
        }
        protected ITimerItem TimerOf(WorkflowItemEvent activityEvent)
        {
            return _allWorkflowItems.OfType<TimerItem>().FirstOrDefault(activityEvent.IsFor);
        }
        IEnumerable<WorkflowItem> IWorkflowItems.GetStartupWorkflowItems()
        {
            return _allWorkflowItems.Where(s => s.HasNoParents());
        }

        IEnumerable<WorkflowItem> IWorkflowItems.GetChildernOf(WorkflowItem item)
        {
            return _allWorkflowItems.Where(s => s.IsChildOf(item));
        }

        WorkflowItem IWorkflowItems.Find(Identity identity)
        {
            return _allWorkflowItems.FirstOrDefault(s => s.Has(identity));
        }
        IWorkflowHistoryEvents IWorkflow.CurrentHistoryEvents
        {
            get
            {
                if(_currentworkflowHistoryEvents==null)
                    throw new InvalidOperationException("Current history events can be accessed only when workflow is executing.");
                return _currentworkflowHistoryEvents;
            }
        }
        WorkflowAction IWorkflowClosingActions.OnCompletion(string result, bool proposal)
        {
            if(proposal && _currentworkflowHistoryEvents.IsActive())
                return WorkflowAction.Ignore;
            return DuringCompletion(result);
        }
        WorkflowAction IWorkflowClosingActions.OnFailure(string reason, string details)
        {
            return DuringFailure(reason,details);
        }

        WorkflowAction IWorkflowClosingActions.OnCancellation(string details)
        {
            return DuringCancellation(details);
        }

        protected virtual WorkflowAction DuringCompletion(string result)
        {
            return CompleteWorkflow(result);
        }
        protected virtual WorkflowAction DuringFailure(string reason, string detail)
        {
            return FailWorkflow(reason,detail);
        }
        protected virtual WorkflowAction DuringCancellation(string details)
        {
            return CancelWorkflow(details);
        }
        internal IEnumerable<WorkflowDecision> ExecuteFor(IWorkflowHistoryEvents workflowHistoryEvents)
        {
            try
            {
                _currentworkflowHistoryEvents = workflowHistoryEvents;
                var workflowDecisions = workflowHistoryEvents.InterpretNewEventsFor(this);
                return FilterOutIncompatibleDecisions(workflowDecisions).Where(d => d != WorkflowDecision.Empty);
            }
            finally
            {
                _currentworkflowHistoryEvents = null;
            }
        }
        private IEnumerable<WorkflowDecision> FilterOutIncompatibleDecisions(IEnumerable<WorkflowDecision> workflowDecisions)
        {
            var compatibleWorkflows = workflowDecisions.Where(d => !d.IsIncompaitbleWith(workflowDecisions.Where(f => !f.Equals(d)))).ToArray();

            var workflowClosingDecisions = compatibleWorkflows.OfType<WorkflowClosingDecision>();
            if (workflowClosingDecisions.Any())
                return workflowClosingDecisions.GenerateFinalDecisionsFor(this);
            
            return compatibleWorkflows;
        } 

        private ActivityItem FindActivity(Identity identity)
        {
            return _allWorkflowItems.OfType<ActivityItem>().FirstOrDefault(a => a.Has(identity));
        }
        private TimerItem FindTimer(Identity identity)
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
                throw new IncompatibleWorkflowException(string.Format("Can not find activity for {0}.", activityEvent));

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
        private WorkflowItem FindWorkflowItemFor(WorkflowItemEvent workflowItemEvent)
        {
            var workflowItem = _allWorkflowItems.FirstOrDefault(workflowItemEvent.IsFor);

            if (workflowItem == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find workflow item for event {0}", workflowItemEvent));

            return workflowItem;
        }
        private ActivityItem FindActivity(WorkflowItemEvent activityEvent)
        {
            return _allWorkflowItems.OfType<ActivityItem>().FirstOrDefault(activityEvent.IsFor);
        }
        private TimerItem FindTimer(WorkflowItemEvent activityEvent)
        {
            return _allWorkflowItems.OfType<TimerItem>().FirstOrDefault(activityEvent.IsFor);
        }
    }
}