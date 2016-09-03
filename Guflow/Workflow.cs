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
        private readonly WorkflowEventMethods _workflowEventMethods;

        protected Workflow()
        {
            _workflowEventMethods = WorkflowEventMethods.For(this);
        }

        WorkflowAction IWorkflowActions.OnWorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.WorkflowStarted);
            return workflowEventMethod == null
                ? WorkflowAction.StartWorkflow(this)
                : workflowEventMethod.Invoke(workflowStartedEvent);
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
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.Signal);
            return workflowEventMethod == null
                ? WorkflowAction.Ignore
                : workflowEventMethod.Invoke(workflowSignaledEvent);
        }
        WorkflowAction IWorkflowActions.OnWorkflowCancellationRequested(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.CancelRequest);
            return workflowEventMethod == null
                ? WorkflowAction.CancelWorkflow(workflowCancellationRequestedEvent.Cause)
                : workflowEventMethod.Invoke(workflowCancellationRequestedEvent);
        }
        WorkflowAction IWorkflowActions.OnRecordMarkerFailed(RecordMarkerFailedEvent recordMarkerFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.RecordMarkerFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_RECORD_MARKER", recordMarkerFailedEvent.Cause)
                : workflowEventMethod.Invoke(recordMarkerFailedEvent);
        }

        public WorkflowAction OnWorkflowSignalFailed(WorkflowSignalFailedEvent workflowSignalFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.SignalFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_SIGNAL_WORKFLOW", workflowSignalFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowSignalFailedEvent);
        }

        public WorkflowAction OnWorkflowCompletionFailed(WorkflowCompletionFailedEvent workflowCompletionFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.CompletionFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_COMPLETE_WORKFLOW", workflowCompletionFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowCompletionFailedEvent);
        }

        public WorkflowAction OnWorkflowFailureFailed(WorkflowFailureFailedEvent workflowFailureFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.FailureFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_FAIL_WORKFLOW", workflowFailureFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowFailureFailedEvent);
        }

        public WorkflowAction OnWorkflowCancelRequestFailed(WorkflowCancelRequestFailedEvent workflowCancelRequestFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.CancelRequestFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_SEND_CANCEL_REQUEST", workflowCancelRequestFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowCancelRequestFailedEvent);
        }

        public WorkflowAction OnWorkflowCancellationFailed(WorkflowCancellationFailedEvent workflowCancellationFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.CancellationFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_CANCEL_WORKFLOW", workflowCancellationFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowCancellationFailedEvent);
        }

        protected IFluentActivityItem ScheduleActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = new ActivityItem(Identity.New(name, version, positionalName), this);
            if (!_allWorkflowItems.Add(activityItem))
                throw new DuplicateItemException(string.Format(Resources.Duplicate_activity, name, version, positionalName));
            return activityItem;
        }
        protected IFluentTimerItem ScheduleTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");

            var timerItem = new TimerItem(Identity.Timer(name), this);
            if (!_allWorkflowItems.Add(timerItem))
                throw new DuplicateItemException(string.Format(Resources.Duplicate_timer, name));

            return timerItem;
        }
        protected WorkflowAction Continue(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");

            var workfowItem = FindWorkflowItemFor(workflowItemEvent);
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
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");
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
        protected ScheduleWorkflowItemAction JumpToActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = FindActivityFor(Identity.New(name, version, positionalName));
            return WorkflowAction.Schedule(activityItem);
        }
        protected WorkflowAction JumpToTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");

            var activityItem = FindTimerFor(Identity.Timer(name));
            return WorkflowAction.Schedule(activityItem);
        }
        protected WorkflowAction CancelActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = FindActivityFor(Identity.New(name, version, positionalName));
            return WorkflowAction.Cancel(activityItem);
        }
        protected WorkflowAction CancelTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");

            var timerItem = FindTimerFor(Identity.Timer(name));
            return WorkflowAction.Cancel(timerItem);
        }

        protected WorkflowAction CancelRequest(string workflowId, string runId = null)
        {
            Ensure.NotNullAndEmpty(workflowId, "workflowId");
            return WorkflowAction.CancelWorkflowRequest(workflowId, runId);
        }
        protected IActivityItem ActivityOf(WorkflowItemEvent activityEvent)
        {
            Ensure.NotNull(activityEvent, "activityEvent");
            return _allWorkflowItems.OfType<ActivityItem>().FirstOrDefault(activityEvent.IsFor);
        }
        protected ITimerItem TimerOf(WorkflowItemEvent activityEvent)
        {
            Ensure.NotNull(activityEvent, "activityEvent");
            return _allWorkflowItems.OfType<TimerItem>().FirstOrDefault(activityEvent.IsFor);
        }
        protected bool IsActive
        {
            get { return ((IWorkflow)this).CurrentHistoryEvents.IsActive(); }
        }
        protected WorkflowAction RecordMarker(string markerName, object details)
        {
            Ensure.NotNullAndEmpty(markerName, "markerName");
            return WorkflowAction.RecordMarker(markerName, details.ToAwsString());
        }
        protected IEnumerable<MarkerRecordedEvent> AllMarkerEvents
        {
            get { return ((IWorkflow)this).CurrentHistoryEvents.AllMarkerRecordedEvents(); }
        }
        protected IEnumerable<WorkflowSignaledEvent> AllSignalEvents
        {
            get { return ((IWorkflow)this).CurrentHistoryEvents.AllSignalEvents(); }
        }
        protected IEnumerable<WorkflowCancellationRequestedEvent> AllCancellationRequestedEvents
        {
            get { return ((IWorkflow)this).CurrentHistoryEvents.AllWorkflowCancellationRequestedEvents(); }
        }
        protected Signal Signal(string signalName, object input)
        {
            Ensure.NotNullAndEmpty(signalName, "signalName");
            return new Signal(signalName, input);
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
                if (_currentworkflowHistoryEvents == null)
                    throw new InvalidOperationException("Current history events can be accessed only when workflow is executing.");
                return _currentworkflowHistoryEvents;
            }
        }
        WorkflowAction IWorkflowClosingActions.OnCompletion(string result, bool proposal)
        {
            if (proposal && IsActive)
                return WorkflowAction.Ignore;
            return DuringCompletion(result);
        }
        WorkflowAction IWorkflowClosingActions.OnFailure(string reason, string details)
        {
            return DuringFailure(reason, details);
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
            return FailWorkflow(reason, detail);
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
                throw new IncompatibleWorkflowException(string.Format("Can not find activity for event {0}.", workflowItemEvent));

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
                throw new WorkflowItemNotFoundException(string.Format("Can not find activity by name {0}, version {1} and positional markerName {2} in workflow.", identity.Name, identity.Version, identity.PositionalName));
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