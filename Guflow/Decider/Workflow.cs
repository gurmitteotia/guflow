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
        private WorkflowEvent _currentExecutingEvent;
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
            eventHandler?.Invoke(this, new WorkflowStartedEventArgs(@event));
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
            completedHandler?.Invoke(this, new WorkflowCompletedEventArgs(workflowId, workflowRunId, result));

            RaiseWorkflowClosedEvent(workflowId, workflowRunId);
        }
        internal void OnFailed(string workflowId, string workflowRunId, string reason, string details)
        {
            var eventHandler = Failed;
            eventHandler?.Invoke(this, new WorkflowFailedEventArgs(workflowId, workflowRunId, reason, details));

            RaiseWorkflowClosedEvent(workflowId, workflowRunId);
        }
        internal void OnCancelled(string workflowId, string workflowRunId, string details)
        {
            var eventHandler = Cancelled;
            eventHandler?.Invoke(this, new WorkflowCancelledEventArgs(workflowId, workflowRunId, details));

            RaiseWorkflowClosedEvent(workflowId, workflowRunId);
        }

        internal void OnRestarted(string workflowId, string workflowRunId)
        {
            var eventHandler = Restarted;
            eventHandler?.Invoke(this, new WorkflowRestartedEventArgs(workflowId, workflowRunId));
            RaiseWorkflowClosedEvent(workflowId, workflowRunId);
        }

        private void RaiseWorkflowClosedEvent(string workflowId, string workflowRunId)
        {
            var closedHandler = Closed;
            closedHandler?.Invoke(this, new WorkflowClosedEventArgs(workflowId, workflowRunId));
        }

        /// <summary>
        /// Schedule the activity given by name and version. Activity should be already registered with Amazon SWF.
        /// </summary>
        /// <param name="name">Name of the activity.</param>
        /// <param name="version">Version of the activity.</param>
        /// <param name="positionalName">A user defined name to differentiate same activity at multiple positions in workflow.</param>
        /// <returns></returns>
        protected IFluentActivityItem ScheduleActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = new ActivityItem(Identity.New(name, version, positionalName), this);
            if (!_allWorkflowItems.Add(activityItem))
                throw new DuplicateItemException(string.Format(Resources.Duplicate_activity, name, version, positionalName));
            return activityItem;
        }
        /// <summary>
        /// Schedule the activity. It reads the activity name and version from <seealso cref="ActivityDescriptionAttribute"/> of TActivity.
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <param name="positionalName">A user defined name to differentiate same activity at multiple positions in workflow</param>
        /// <returns></returns>
        protected IFluentActivityItem ScheduleActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescriptionAttribute.FindOn<TActivity>();
            return ScheduleActivity(description.Name, description.Version, positionalName);
        }
        /// <summary>
        /// Schedule the timer.
        /// </summary>
        /// <param name="name">Any user defined name to assign to this timer.</param>
        /// <returns></returns>
        protected IFluentTimerItem ScheduleTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");

            var timerItem = TimerItem.New(Identity.Timer(name), this);
            if (!_allWorkflowItems.Add(timerItem))
                throw new DuplicateItemException(string.Format(Resources.Duplicate_timer, name));

            return timerItem;
        }
        /// <summary>
        /// Schedule a workflow action directly.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        protected IFluentWorkflowActionItem ScheduleAction(Func<IWorkflowItem,WorkflowAction> workflowAction)
        {
            var workflowActionItem = new WorkflowActionItem(workflowAction, this);
            _allWorkflowItems.Add(workflowActionItem);
            return workflowActionItem;
        }
        /// <summary>
        /// Continue the scheduling the of child items. All child items will be scheduled as per Deflow algorithm.
        /// </summary>
        /// <param name="workflowItemEvent"></param>
        /// <returns></returns>
        protected WorkflowAction Continue(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");

            var workfowItem = _allWorkflowItems.WorkflowItemFor(workflowItemEvent);
            return WorkflowAction.ContinueWorkflow(workfowItem);
        }
        /// <summary>
        /// Fail workflow with given reason and details. It will cause the workflow to be closed immediately on Amazon SWF with failed status.
        /// </summary>
        /// <param name="reason">Short reason, why workflow is failing.</param>
        /// <param name="details">Any detail about failure.</param>
        /// <returns></returns>
        protected static WorkflowAction FailWorkflow(string reason, string details)
        {
            return WorkflowAction.FailWorkflow(reason, details);
        }
        /// <summary>
        /// Complete the workflow with given result. It will cause the workflow to be closed immediately on Amazon SWF with completed status.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected static WorkflowAction CompleteWorkflow(string result)
        {
            return WorkflowAction.CompleteWorkflow(result);
        }
        /// <summary>
        /// Cancel the workflow. It will cause the workflow to be closed immediately on Amazon SWF with cancelled status.
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        protected static WorkflowAction CancelWorkflow(string details)
        {
            return WorkflowAction.CancelWorkflow(details);
        }
        /// <summary>
        /// Close the current workflow and restart it again.
        /// </summary>
        /// <returns></returns>
        protected RestartWorkflowAction RestartWorkflow()
        {
            IWorkflow workflow = this;
            return WorkflowAction.RestartWorkflow(workflow.WorkflowHistoryEvents);
        }
        /// <summary>
        /// Reschedule the item, associated with passed event, again.
        /// </summary>
        /// <param name="workflowItemEvent"></param>
        /// <returns></returns>
        protected ScheduleWorkflowItemAction Reschedule(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");
            var workflowItem = _allWorkflowItems.WorkflowItemFor(workflowItemEvent);
            return WorkflowAction.Schedule(workflowItem);
        }
        /// <summary>
        /// Start the workflow by scheduling all the startup item. This action is different than RestartWorkflow. Later one will restart the workflow
        /// by closing the current workflow execution. While this action will not close the workflow and will only schedule the start up item.
        /// </summary>
        /// <returns></returns>
        protected WorkflowAction StartWorkflow()
        {
            return WorkflowAction.StartWorkflow(_allWorkflowItems);
        }
        /// <summary>
        /// Ignore the event and do not take any action.
        /// </summary>
        /// <param name="keepBranchActive"></param>
        /// <returns></returns>
        protected static WorkflowAction Ignore(bool keepBranchActive)
        {
            return WorkflowAction.Ignore(keepBranchActive);
        }
        /// <summary>
        /// Provides methods to jump to schedulable items in workflow. It will cause target item to schedule immediatly(or after timeout) without checking
        /// for When condition.
        /// </summary>
        protected JumpActions Jump => CurrentExecutingItem != null
            ? JumpActions.FromWorkflowItem(_allWorkflowItems, CurrentExecutingItem)
            : JumpActions.FromWorkflowEvent(_allWorkflowItems);

        /// <summary>
        /// Helps in trigger the scheduling the joint item.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <returns></returns>
        protected TriggerActions Trigger(IWorkflowItem workflowItem)
        {
            var item = workflowItem as WorkflowItem;
            Ensure.NotNull(item, nameof(workflowItem));
            return new TriggerActions(item);
        }
        /// <summary>
        /// Return default action for event.
        /// </summary>
        /// <param name="workflowEvent"></param>
        /// <returns></returns>
        protected WorkflowAction DefaultAction(WorkflowEvent workflowEvent)
        {
            Ensure.NotNull(workflowEvent, "workflowEvent");
            IWorkflowDefaultActions defaultActions = this;
            return workflowEvent.DefaultAction(defaultActions);
        }
        /// <summary>
        /// Supports cancelling the the activity, timer and workflows.
        /// </summary>
        protected CancelRequest CancelRequest => new CancelRequest(_allWorkflowItems);

        /// <summary>
        /// Returns activity of passed event.
        /// </summary>
        /// <param name="activityEvent"></param>
        /// <returns></returns>
        protected IActivityItem ActivityOf(WorkflowItemEvent activityEvent)
        {
            Ensure.NotNull(activityEvent, "activityEvent");
            return _allWorkflowItems.ActivityOf(activityEvent);
        }

        /// <summary>
        /// Returns timer of passed event.
        /// </summary>
        /// <param name="activityEvent"></param>
        /// <returns></returns>
        protected ITimerItem TimerOf(WorkflowItemEvent activityEvent)
        {
            Ensure.NotNull(activityEvent, "activityEvent");
            return _allWorkflowItems.TimerItemFor(activityEvent);
        }
        /// <summary>
        /// All child items of workflow.
        /// </summary>
        protected IEnumerable<IWorkflowItem> WorkflowItems => _allWorkflowItems.AllItems;
        /// <summary>
        /// Returns all activities of workflows.
        /// </summary>
        protected IEnumerable<IActivityItem> Activities => _allWorkflowItems.AllActivities;
        /// <summary>
        /// All schedulable timers of workflows.
        /// </summary>
        protected IEnumerable<ITimerItem> Timers => _allWorkflowItems.AllTimers;

        /// <summary>
        /// Indicate if workflow history event has any active event. An active event indicates that a scheduling item (activity, timer...) is active. e.g. if an activity is just started but not finished/failed/cancelled
        /// then it is an active event.
        /// </summary>
        protected bool HasActiveEvent => ((IWorkflow)this).WorkflowHistoryEvents.HasActiveEvent();
        /// <summary>
        /// Record a marker in workflow history event.
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        protected WorkflowAction RecordMarker(string markerName, object details)
        {
            Ensure.NotNullAndEmpty(markerName, "markerName");
            return WorkflowAction.RecordMarker(markerName, details.ToAwsString());
        }
        /// <summary>
        /// Returns all marker events from workflow history events.
        /// </summary>
        protected IEnumerable<MarkerRecordedEvent> AllMarkerEvents => ((IWorkflow)this).WorkflowHistoryEvents.AllMarkerRecordedEvents();

        /// <summary>
        /// Returns all signal events from workflow history events.
        /// </summary>
        protected IEnumerable<WorkflowSignaledEvent> AllSignalEvents => ((IWorkflow)this).WorkflowHistoryEvents.AllSignalEvents();

        /// <summary>
        /// Returns all cancelletion request made to this workflow.
        /// </summary>
        protected IEnumerable<WorkflowCancellationRequestedEvent> AllCancellationRequestedEvents
        {
            get { return ((IWorkflow)this).WorkflowHistoryEvents.AllWorkflowCancellationRequestedEvents(); }
        }
        /// <summary>
        /// Supports sending signal to other workflows.
        /// </summary>
        /// <param name="signalName"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected static Signal Signal(string signalName, object input)
        {
            Ensure.NotNullAndEmpty(signalName, "signalName");
            return new Signal(signalName, input);
        }
        /// <summary>
        /// Return workflow input as dynamic object. If workflow input is JSON data then you can directly access the properties like: Input.Session.
        /// </summary>
        protected dynamic Input => StartedEvent.Input.FromJson();

        /// <summary>
        /// Return workflow input as TType object.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        protected TType InputAs<TType>()
        {
            return StartedEvent.Input.FromJson<TType>();
        }

        /// <summary>
        /// Return workflow started event.
        /// </summary>
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
        void IWorkflow.SetCurrentExecutingEvent(WorkflowEvent workflowEvent)
        {
            _currentExecutingEvent = workflowEvent;
        }
        private WorkflowItem CurrentExecutingItem
        {
            get
            {
                var workflowItemEvent = _currentExecutingEvent as WorkflowItemEvent;
                if (workflowItemEvent != null)
                    return _allWorkflowItems.WorkflowItemFor(workflowItemEvent);

                return null;
            }
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
        internal WorkflowAction StartupAction => _startupAction ?? (_startupAction = WorkflowAction.StartWorkflow(_allWorkflowItems));

        WorkflowAction IWorkflowClosingActions.OnCompletion(string result, bool proposal)
        {
            if (proposal && HasActiveEvent)
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