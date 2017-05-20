using System;
using System.Collections.Generic;
using Guflow.Properties;

namespace Guflow.Decider
{
    public abstract class Workflow : IWorkflow, IWorkflowClosingActions
    {
        private readonly WorkflowItems _allWorkflowItems = new WorkflowItems();
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
            IActivity activity = _allWorkflowItems.ActivityItemFor(activityCompletedEvent);
            return activity.Completed(activityCompletedEvent);
        }
        WorkflowAction IWorkflowActions.OnActivityFailure(ActivityFailedEvent activityFailedEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItemFor(activityFailedEvent);
            return activity.Failed(activityFailedEvent);
        }
        WorkflowAction IWorkflowActions.OnActivityTimeout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItemFor(activityTimedoutEvent);
            return activity.Timedout(activityTimedoutEvent);
        }
        WorkflowAction IWorkflowActions.OnActivityCancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItemFor(activityCancelledEvent);
            return activity.Cancelled(activityCancelledEvent);
        }
        WorkflowAction IWorkflowActions.OnActivityCancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            IActivity workflowActivity = _allWorkflowItems.ActivityItemFor(activityCancellationFailedEvent);
            return workflowActivity.CancellationFailed(activityCancellationFailedEvent);
        }

        WorkflowAction IWorkflowActions.OnActivitySchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent)
        {
            IActivity workflowActivity = _allWorkflowItems.ActivityItemFor(activitySchedulingFailedEvent);
            return workflowActivity.SchedulingFailed(activitySchedulingFailedEvent);
        }
        WorkflowAction IWorkflowActions.OnTimerFired(TimerFiredEvent timerFiredEvent)
        {
            ITimer timer = _allWorkflowItems.TimerFor(timerFiredEvent);
            return timer.Fired(timerFiredEvent);
        }
        WorkflowAction IWorkflowActions.OnTimerStartFailure(TimerStartFailedEvent timerStartFailedEvent)
        {
            ITimer timer = _allWorkflowItems.TimerFor(timerStartFailedEvent);
            return timer.StartFailed(timerStartFailedEvent);
        }
        WorkflowAction IWorkflowActions.OnTimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            ITimer timer = _allWorkflowItems.TimerFor(timerCancelledEvent);
            return timer.Cancelled(timerCancelledEvent);
        }
        WorkflowAction IWorkflowActions.OnTimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            ITimer timer = _allWorkflowItems.TimerFor(timerCancellationFailedEvent);
            return timer.CancellationFailed(timerCancellationFailedEvent);
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

            var timerItem = TimerItem.New(Identity.Timer(name), this);
            if (!_allWorkflowItems.Add(timerItem))
                throw new DuplicateItemException(string.Format(Resources.Duplicate_timer, name));

            return timerItem;
        }

        protected IFluentWorkflowActionItem ScheduleAction(WorkflowAction workflowAction)
        {
            var workflowActionItem = new WorkflowActionItem(workflowAction, this);
            _allWorkflowItems.Add(workflowActionItem);
            return workflowActionItem;
        }
        protected WorkflowAction Continue(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");

            var workfowItem = _allWorkflowItems.WorkflowItemFor(workflowItemEvent);
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
            var workflowItem = _allWorkflowItems.WorkflowItemFor(workflowItemEvent);
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

            var activityItem = _allWorkflowItems.ActivityItemFor(Identity.New(name, version, positionalName));
            return WorkflowAction.Schedule(activityItem);
        }
        protected WorkflowAction JumpToTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");

            var activityItem = _allWorkflowItems.TimerItemFor(Identity.Timer(name));
            return WorkflowAction.Schedule(activityItem);
        }

        protected CancelRequest CancelRequest { get { return new CancelRequest(this);} }

        protected IActivityItem ActivityOf(WorkflowItemEvent activityEvent)
        {
            Ensure.NotNull(activityEvent, "activityEvent");
            return _allWorkflowItems.ActivityOf(activityEvent);
        }
        protected ITimerItem TimerOf(WorkflowItemEvent activityEvent)
        {
            Ensure.NotNull(activityEvent, "activityEvent");
            return _allWorkflowItems.TimerItemFor(activityEvent);
        }
        protected IEnumerable<IWorkflowItem> AllWorkflowItems { get { return _allWorkflowItems.AllItems ; } }
        protected IEnumerable<IActivityItem> AllActivities { get { return _allWorkflowItems.AllActivities; } }
        protected IEnumerable<ITimerItem> AllTimers { get { return _allWorkflowItems.AllTimers; } }
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
            return _allWorkflowItems.StartupItems();
        }
        IEnumerable<WorkflowItem> IWorkflowItems.GetChildernOf(WorkflowItem item)
        {
            return _allWorkflowItems.ChilderenOf(item);
        }
        WorkflowItem IWorkflowItems.Find(Identity identity)
        {
            return _allWorkflowItems.WorkflowItemFor(identity);
        }
        ActivityItem IWorkflowItems.FindActivityFor(Identity identity)
        {
            return _allWorkflowItems.ActivityItemFor(identity);
        }
        TimerItem IWorkflowItems.FindTimerFor(Identity identity)
        {
            return _allWorkflowItems.TimerItemFor(identity);
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
    }
}