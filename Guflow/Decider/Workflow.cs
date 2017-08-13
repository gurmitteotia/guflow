using System;
using System.Collections.Generic;
using Guflow.Properties;
using Guflow.Worker;

namespace Guflow.Decider
{
    public abstract class Workflow : IWorkflow, IWorkflowClosingActions
    {
        private readonly WorkflowItems _allWorkflowItems = new WorkflowItems();
        private IWorkflowHistoryEvents _currentWorkflowHistoryEvents;
        private readonly WorkflowEventMethods _workflowEventMethods;
        private WorkflowAction _startupAction;

        protected Workflow()
        {
            _workflowEventMethods = WorkflowEventMethods.For(this);
        }
        public event EventHandler<WorkflowCompletedEventArgs> Completed;
        public event EventHandler<WorkflowFailedEventArgs> Failed;
        public event EventHandler<WorkflowCancelledEventArgs> Cancelled;
        public event EventHandler<WorkflowStartedEventArgs> Started;
        public event EventHandler<WorkflowClosedEventArgs> Closed;
        public event EventHandler<WorkflowRestartedEventArgs> Restarted;

        WorkflowAction IWorkflow.OnActivityCompletion(ActivityCompletedEvent activityCompletedEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItemFor(activityCompletedEvent);
            return activity.Completed(activityCompletedEvent);
        }
        WorkflowAction IWorkflow.OnActivityFailure(ActivityFailedEvent activityFailedEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItemFor(activityFailedEvent);
            return activity.Failed(activityFailedEvent);
        }
        WorkflowAction IWorkflow.OnActivityTimeout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItemFor(activityTimedoutEvent);
            return activity.Timedout(activityTimedoutEvent);
        }
        WorkflowAction IWorkflow.OnActivityCancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItemFor(activityCancelledEvent);
            return activity.Cancelled(activityCancelledEvent);
        }
        WorkflowAction IWorkflow.OnActivityCancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            IActivity workflowActivity = _allWorkflowItems.ActivityItemFor(activityCancellationFailedEvent);
            return workflowActivity.CancellationFailed(activityCancellationFailedEvent);
        }

        WorkflowAction IWorkflow.OnActivitySchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent)
        {
            IActivity workflowActivity = _allWorkflowItems.ActivityItemFor(activitySchedulingFailedEvent);
            return workflowActivity.SchedulingFailed(activitySchedulingFailedEvent);
        }
        WorkflowAction IWorkflow.OnTimerFired(TimerFiredEvent timerFiredEvent)
        {
            ITimer timer = _allWorkflowItems.TimerFor(timerFiredEvent);
            return timer.Fired(timerFiredEvent);
        }
        WorkflowAction IWorkflow.OnTimerStartFailure(TimerStartFailedEvent timerStartFailedEvent)
        {
            ITimer timer = _allWorkflowItems.TimerFor(timerStartFailedEvent);
            return timer.StartFailed(timerStartFailedEvent);
        }
        WorkflowAction IWorkflow.OnTimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            ITimer timer = _allWorkflowItems.TimerFor(timerCancelledEvent);
            return timer.Cancelled(timerCancelledEvent);
        }
        WorkflowAction IWorkflow.OnTimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            ITimer timer = _allWorkflowItems.TimerFor(timerCancellationFailedEvent);
            return timer.CancellationFailed(timerCancellationFailedEvent);
        }
        WorkflowAction IWorkflow.OnWorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            Raise(workflowStartedEvent);
            return Handle(EventName.WorkflowStarted, workflowStartedEvent);
        }
        private void Raise(WorkflowStartedEvent @event)
        {
            var eventHandler = Started;
            if (eventHandler != null)
                eventHandler(this, new WorkflowStartedEventArgs(@event));
        }
        WorkflowAction IWorkflow.OnWorkflowSignaled(WorkflowSignaledEvent workflowSignaledEvent)
        {
            return Handle(EventName.Signal, workflowSignaledEvent);
        }
        WorkflowAction IWorkflow.OnWorkflowCancellationRequested(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent)
        {
            return Handle(EventName.CancelRequest, workflowCancellationRequestedEvent);
        }
        WorkflowAction IWorkflow.OnRecordMarkerFailed(RecordMarkerFailedEvent recordMarkerFailedEvent)
        {
            return Handle(EventName.RecordMarkerFailed, recordMarkerFailedEvent);
        }

        WorkflowAction IWorkflow.OnWorkflowSignalFailed(WorkflowSignalFailedEvent workflowSignalFailedEvent)
        {
            return Handle(EventName.SignalFailed, workflowSignalFailedEvent);
        }

        WorkflowAction IWorkflow.OnWorkflowCompletionFailed(WorkflowCompletionFailedEvent workflowCompletionFailedEvent)
        {
            return Handle(EventName.CompletionFailed, workflowCompletionFailedEvent);
        }

        WorkflowAction IWorkflow.OnWorkflowFailureFailed(WorkflowFailureFailedEvent workflowFailureFailedEvent)
        {
            return Handle(EventName.FailureFailed, workflowFailureFailedEvent);
        }

        WorkflowAction IWorkflow.OnWorkflowCancelRequestFailed(WorkflowCancelRequestFailedEvent workflowCancelRequestFailedEvent)
        {
            return Handle(EventName.CancelRequestFailed, workflowCancelRequestFailedEvent);
        }

        WorkflowAction IWorkflow.OnWorkflowCancellationFailed(WorkflowCancellationFailedEvent workflowCancellationFailedEvent)
        {
            return Handle(EventName.CancellationFailed, workflowCancellationFailedEvent);
        }

        private WorkflowAction Handle(EventName eventName, WorkflowEvent workflowEvent)
        {
            var workflowEventMethod = _workflowEventMethods.FindFor(eventName);
            return workflowEventMethod == null
                ? workflowEvent.DefaultAction(this)
                : workflowEventMethod.Invoke(workflowEvent);
        }
        WorkflowAction IWorkflowDefaultActions.Continue(WorkflowItemEvent workflowItemEvent)
        {
            return Continue(workflowItemEvent);
        }

        WorkflowAction IWorkflowDefaultActions.StartWorkflow()
        {
            return StartWorkflow();
        }

        WorkflowAction IWorkflowDefaultActions.FailWorkflow(string reason, string details)
        {
            return FailWorkflow(reason, details);
        }

        WorkflowAction IWorkflowDefaultActions.CancelWorkflow(string details)
        {
            return CancelWorkflow(details);
        }

        WorkflowAction IWorkflowDefaultActions.Ignore()
        {
            return Ignore(false);
        }
        internal void OnCompleted(string workflowId, string workflowRunId, string result)
        {
            var completedHandler = Completed;
            if (completedHandler != null)
                completedHandler(this, new WorkflowCompletedEventArgs(workflowId, workflowRunId, result));

            RaiseWorkflowClosedEvent(workflowId, workflowRunId);
        }
        internal void OnFailed(string workflowId, string workflowRunId, string reason, string details)
        {
            var eventHandler = Failed;
            if (eventHandler != null)
                eventHandler(this, new WorkflowFailedEventArgs(workflowId, workflowRunId, reason, details));

            RaiseWorkflowClosedEvent(workflowId, workflowRunId);
        }
        internal void OnCancelled(string workflowId, string workflowRunId, string details)
        {
            var eventHandler = Cancelled;
            if (eventHandler != null)
                eventHandler(this, new WorkflowCancelledEventArgs(workflowId, workflowRunId, details));

            RaiseWorkflowClosedEvent(workflowId, workflowRunId);
        }

        internal void OnRestarted(string workflowId, string workflowRunId)
        {
            var eventHandler = Restarted;
            if (eventHandler != null)
                eventHandler(this, new WorkflowRestartedEventArgs(workflowId, workflowRunId));
            RaiseWorkflowClosedEvent(workflowId, workflowRunId);
        }

        private void RaiseWorkflowClosedEvent(string workflowId, string workflowRunId)
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
        protected IFluentActivityItem ScheduleActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescriptionAttribute.FindOn<TActivity>();
            return ScheduleActivity(description.Name, description.Version, positionalName);
        }
        protected IFluentTimerItem ScheduleTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");

            var timerItem = TimerItem.New(Identity.Timer(name), this);
            if (!_allWorkflowItems.Add(timerItem))
                throw new DuplicateItemException(string.Format(Resources.Duplicate_timer, name));

            return timerItem;
        }

        protected IFluentWorkflowActionItem ScheduleAction(Func<IWorkflowItem,WorkflowAction> workflowAction)
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
        protected RestartWorkflowAction RestartWorkflow()
        {
            IWorkflow workflow = this;
            return WorkflowAction.RestartWorkflow(workflow.WorkflowHistoryEvents);
        }
        protected ScheduleWorkflowItemAction Reschedule(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");
            var workflowItem = _allWorkflowItems.WorkflowItemFor(workflowItemEvent);
            return WorkflowAction.Schedule(workflowItem);
        }
        protected WorkflowAction StartWorkflow()
        {
            return WorkflowAction.StartWorkflow(_allWorkflowItems);
        }
        protected static WorkflowAction Ignore(bool keepBranchActive)
        {
            return WorkflowAction.Ignore(keepBranchActive);
        }

        protected JumpAction Jump(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");
            var workflowItem = _allWorkflowItems.WorkflowItemFor(workflowItemEvent);

            return JumpAction.JumpFromItem(workflowItem, _allWorkflowItems);
        }
        protected JumpAction Jump()
        {
                return JumpAction.JumpFromNonItem(_allWorkflowItems);
        }
        protected WorkflowAction DefaultAction(WorkflowEvent workflowEvent)
        {
            Ensure.NotNull(workflowEvent, "workflowEvent");
            IWorkflowDefaultActions defaultActions = this;
            return workflowEvent.DefaultAction(defaultActions);
        }
        protected CancelRequest CancelRequest { get { return new CancelRequest(_allWorkflowItems); } }

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
        protected IEnumerable<IWorkflowItem> WorkflowItems { get { return _allWorkflowItems.AllItems; } }
        protected IEnumerable<IActivityItem> Activities { get { return _allWorkflowItems.AllActivities; } }
        protected IEnumerable<ITimerItem> Timers { get { return _allWorkflowItems.AllTimers; } }
        protected bool IsActive
        {
            get { return ((IWorkflow)this).WorkflowHistoryEvents.IsActive(); }
        }
        protected WorkflowAction RecordMarker(string markerName, object details)
        {
            Ensure.NotNullAndEmpty(markerName, "markerName");
            return WorkflowAction.RecordMarker(markerName, details.ToAwsString());
        }
        protected IEnumerable<MarkerRecordedEvent> AllMarkerEvents
        {
            get { return ((IWorkflow)this).WorkflowHistoryEvents.AllMarkerRecordedEvents(); }
        }
        protected IEnumerable<WorkflowSignaledEvent> AllSignalEvents
        {
            get { return ((IWorkflow)this).WorkflowHistoryEvents.AllSignalEvents(); }
        }
        protected IEnumerable<WorkflowCancellationRequestedEvent> AllCancellationRequestedEvents
        {
            get { return ((IWorkflow)this).WorkflowHistoryEvents.AllWorkflowCancellationRequestedEvents(); }
        }
        protected static Signal Signal(string signalName, object input)
        {
            Ensure.NotNullAndEmpty(signalName, "signalName");
            return new Signal(signalName, input);
        }

        protected dynamic Input
        {
            get
            {
               return StartedEvent.Input.FromJson();
            }
        }
        protected TType InputAs<TType>()
        {
            return StartedEvent.Input.FromJson<TType>();
        }
        protected WorkflowStartedEvent StartedEvent
        {
            get
            {
                IWorkflow workflow = this;
                return workflow.WorkflowHistoryEvents.WorkflowStartedEvent();
            }
        }
        IEnumerable<WorkflowItem> IWorkflow.GetChildernOf(WorkflowItem workflowItem)
        {
            return _allWorkflowItems.ChilderenOf(workflowItem);
        }

        WorkflowItem IWorkflow.FindWorkflowItemBy(Identity identity)
        {
            return _allWorkflowItems.WorkflowItemFor(identity);
        }

        IWorkflowHistoryEvents IWorkflow.WorkflowHistoryEvents
        {
            get
            {
                if (_currentWorkflowHistoryEvents == null)
                    throw new InvalidOperationException("Current history events can be accessed only when workflow is executing.");
                return _currentWorkflowHistoryEvents;
            }
        }
        internal WorkflowAction StartupAction
        {
            get { return _startupAction ?? (_startupAction = WorkflowAction.StartWorkflow(_allWorkflowItems)); }
        }
        WorkflowAction IWorkflowClosingActions.OnCompletion(string result, bool proposal)
        {
            if (proposal && IsActive)
                return WorkflowAction.Empty;
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
        internal WorkflowEventsExecution NewExecutionFor(IWorkflowHistoryEvents workflowHistoryEvents)
        {
            _currentWorkflowHistoryEvents = workflowHistoryEvents;
            return new WorkflowEventsExecution(this, workflowHistoryEvents);
        }
        internal void FinishExecution()
        {
            _currentWorkflowHistoryEvents = null;
        }
    }
}