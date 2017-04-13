using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Decider
{
    public abstract class Workflow : IWorkflow, IWorkflowClosingActions
    {
        private readonly HashSet<WorkflowItem> _allWorkflowItems = new HashSet<WorkflowItem>();
        private IWorkflowEvents _currentWorkflowEvents;
        private readonly WorkflowEventMethods _workflowEventMethods;

        protected Workflow()
        {
            _workflowEventMethods = WorkflowEventMethods.For(this);
        }
        public event EventHandler<WorkflowCompletedEventArgs> Completed;
        public event EventHandler<WorkflowFailedEventArgs> Failed;
        public event EventHandler<WorkflowCancelledEventArgs> Cancelled;
        public event EventHandler<WorkflowStartedEventArgs> Started;
        public event EventHandler<WorkflowClosedEventArgs> Closed;

        WorkflowAction IWorkflowActions.OnWorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            Raise(workflowStartedEvent);

            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.WorkflowStarted);
            return workflowEventMethod == null
                ? WorkflowAction.StartWorkflow(this)
                : workflowEventMethod.Invoke(workflowStartedEvent);
        }

        private void Raise(WorkflowStartedEvent @event)
        {
            var eventHandler = Started;
            if(eventHandler!=null)
                eventHandler(this,new WorkflowStartedEventArgs(@event));
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

        WorkflowAction IWorkflowActions.OnWorkflowSignalFailed(WorkflowSignalFailedEvent workflowSignalFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.SignalFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_SIGNAL_WORKFLOW", workflowSignalFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowSignalFailedEvent);
        }

        WorkflowAction IWorkflowActions.OnWorkflowCompletionFailed(WorkflowCompletionFailedEvent workflowCompletionFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.CompletionFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_COMPLETE_WORKFLOW", workflowCompletionFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowCompletionFailedEvent);
        }

        WorkflowAction IWorkflowActions.OnWorkflowFailureFailed(WorkflowFailureFailedEvent workflowFailureFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.FailureFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_FAIL_WORKFLOW", workflowFailureFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowFailureFailedEvent);
        }

        WorkflowAction IWorkflowActions.OnWorkflowCancelRequestFailed(WorkflowCancelRequestFailedEvent workflowCancelRequestFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.CancelRequestFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_SEND_CANCEL_REQUEST", workflowCancelRequestFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowCancelRequestFailedEvent);
        }

        WorkflowAction IWorkflowActions.OnWorkflowCancellationFailed(WorkflowCancellationFailedEvent workflowCancellationFailedEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(EventName.CancellationFailed);
            return workflowEventMethod == null
                ? FailWorkflow("FAILED_TO_CANCEL_WORKFLOW", workflowCancellationFailedEvent.Cause)
                : workflowEventMethod.Invoke(workflowCancellationFailedEvent);
        }

        internal void OnCompleted(string workflowId, string workflowRunId, string result)
        {
            var completedHandler = Completed;
            if(completedHandler!=null)
                completedHandler(this, new WorkflowCompletedEventArgs(workflowId, workflowRunId, result));

            RaiseClosed(workflowId, workflowRunId);
        }
        internal void OnFailed(string workflowId, string workflowRunId, string reason, string details)
        {
            var eventHandler = Failed;
            if (eventHandler != null)
                eventHandler(this, new WorkflowFailedEventArgs(workflowId, workflowRunId, reason, details));

            RaiseClosed(workflowId, workflowRunId);
        }
        internal void OnCancelled(string workflowId, string workflowRunId, string details)
        {
            var eventHandler = Cancelled;
            if (eventHandler != null)
                eventHandler(this, new WorkflowCancelledEventArgs(workflowId, workflowRunId, details));

            RaiseClosed(workflowId, workflowRunId);
        }

        private void RaiseClosed(string workflowId, string workflowRunId)
        {
            var closedHandler = Closed;
            if (closedHandler != null)
                closedHandler(this, new WorkflowClosedEventArgs(workflowId, workflowRunId));
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

        protected IFluentWorkflowActionItem ScheduleAction(WorkflowAction workflowAction)
        {
            return new WorkflowActionItem(workflowAction);
        }
        protected WorkflowAction Continue(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");

            var workfowItem = FindWorkflowItemFor(workflowItemEvent);
            return WorkflowAction.ContinueWorkflow(workfowItem);
        }
        protected static WorkflowAction FailWorkflow(string reason, string details)
        {
            return WorkflowAction.FailWorkflow(reason, details);
        }
        protected static WorkflowAction CompleteWorkflow(string result)
        {
            return WorkflowAction.CompleteWorkflow(result);
        }
        protected static WorkflowAction CancelWorkflow(string details)
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
        protected static WorkflowAction Ignore()
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

        protected CancelRequest CancelRequest { get { return new CancelRequest(this);} }

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
        protected IEnumerable<IWorkflowItem> AllWorkflowItems { get { return _allWorkflowItems; } }
        protected IEnumerable<IActivityItem> AllActivities { get { return _allWorkflowItems.OfType<IActivityItem>(); } }
        protected IEnumerable<ITimerItem> AllTimers { get { return _allWorkflowItems.OfType<ITimerItem>(); } }
        protected bool IsActive
        {
            get { return ((IWorkflow)this).WorkflowEvents.IsActive(); }
        }
        protected WorkflowAction RecordMarker(string markerName, object details)
        {
            Ensure.NotNullAndEmpty(markerName, "markerName");
            return WorkflowAction.RecordMarker(markerName, details.ToAwsString());
        }
        protected IEnumerable<MarkerRecordedEvent> AllMarkerEvents
        {
            get { return ((IWorkflow)this).WorkflowEvents.AllMarkerRecordedEvents(); }
        }
        protected IEnumerable<WorkflowSignaledEvent> AllSignalEvents
        {
            get { return ((IWorkflow)this).WorkflowEvents.AllSignalEvents(); }
        }
        protected IEnumerable<WorkflowCancellationRequestedEvent> AllCancellationRequestedEvents
        {
            get { return ((IWorkflow)this).WorkflowEvents.AllWorkflowCancellationRequestedEvents(); }
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
        ActivityItem IWorkflowItems.FindActivityFor(Identity identity)
        {
            return FindActivityFor(identity);
        }
        TimerItem IWorkflowItems.FindTimerFor(Identity identity)
        {
            return FindTimerFor(identity);
        }
        IWorkflowEvents IWorkflow.WorkflowEvents
        {
            get
            {
                if (_currentWorkflowEvents == null)
                    throw new InvalidOperationException("Current history events can be accessed only when workflow is executing.");
                return _currentWorkflowEvents;
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

        internal WorkflowEventsExecution NewExecutionFor(IWorkflowEvents workflowEvents)
        {
            _currentWorkflowEvents = workflowEvents;
            return new WorkflowEventsExecution(this, workflowEvents);
        }

        internal void FinishExecution()
        {
            _currentWorkflowEvents = null;
        }

        private ActivityItem FindActivity(Identity identity)
        {
            return _allWorkflowItems.OfType<ActivityItem>().FirstOrDefault(a => a.Has(identity));
        }
        private TimerItem FindTimer(Identity identity)
        {
            return _allWorkflowItems.OfType<TimerItem>().FirstOrDefault(s => s.Has(identity));
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
    }
}