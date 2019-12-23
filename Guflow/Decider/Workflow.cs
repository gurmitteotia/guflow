// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;
using Guflow.Worker;

namespace Guflow.Decider
{
    /// <summary>
    /// Represents a workflow to schedule its children in Amazon SWF. Derive from this class to create custom workflow.
    /// </summary>
    public abstract class Workflow : IWorkflow, IWorkflowClosingActions
    {
        private readonly WorkflowItems _allWorkflowItems = new WorkflowItems();
        private IWorkflowHistoryEvents _currentWorkflowHistoryEvents;
        private readonly WorkflowEventMethods _workflowEventMethods;
        private WorkflowAction _startupAction;
        
        private WorkflowActions _generatedActions;
        protected Workflow()
        {
            _workflowEventMethods = WorkflowEventMethods.For(this);
        }
        /// <summary>
        /// Raised when workflow is sucessfully completed in Amazon SWF.
        /// </summary>
        public event EventHandler<WorkflowCompletedEventArgs> Completed;
        /// <summary>
        /// Raised when workflow is failed in Amazon SWF.
        /// </summary>
        public event EventHandler<WorkflowFailedEventArgs> Failed;
        /// <summary>
        /// Raised when workflow is cancelled.
        /// </summary>
        public event EventHandler<WorkflowCancelledEventArgs> Cancelled;
        /// <summary>
        /// Raised when workflow is started.
        /// </summary>
        public event EventHandler<WorkflowStartedEventArgs> Started;
        /// <summary>
        /// Raised when workflow is closed because it is either failed, completed or cancelled.
        /// </summary>
        public event EventHandler<WorkflowClosedEventArgs> Closed;
        /// <summary>
        /// Raised when workflow is restarted.
        /// </summary>
        public event EventHandler<WorkflowRestartedEventArgs> Restarted;

        WorkflowAction IWorkflow.WorkflowAction(ActivityCompletedEvent activityCompletedEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItem(activityCompletedEvent);
            return activity.Completed(activityCompletedEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(ActivityFailedEvent activityFailedEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItem(activityFailedEvent);
            return activity.Failed(activityFailedEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(ActivityTimedoutEvent activityTimedoutEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItem(activityTimedoutEvent);
            return activity.Timedout(activityTimedoutEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(ActivityCancelledEvent activityCancelledEvent)
        {
            IActivity activity = _allWorkflowItems.ActivityItem(activityCancelledEvent);
            return activity.Cancelled(activityCancelledEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            IActivity workflowActivity = _allWorkflowItems.ActivityItem(activityCancellationFailedEvent);
            return workflowActivity.CancellationFailed(activityCancellationFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ActivitySchedulingFailedEvent activitySchedulingFailedEvent)
        {
            IActivity workflowActivity = _allWorkflowItems.ActivityItem(activitySchedulingFailedEvent);
            return workflowActivity.SchedulingFailed(activitySchedulingFailedEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(TimerFiredEvent timerFiredEvent)
        {
            ITimer timer = _allWorkflowItems.Timer(timerFiredEvent);
            return timer.Fired(timerFiredEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(TimerStartFailedEvent timerStartFailedEvent)
        {
            ITimer timer = _allWorkflowItems.Timer(timerStartFailedEvent);
            return timer.StartFailed(timerStartFailedEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            ITimer timer = _allWorkflowItems.Timer(timerCancellationFailedEvent);
            return timer.CancellationFailed(timerCancellationFailedEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(WorkflowStartedEvent workflowStartedEvent)
        {
            Raise(workflowStartedEvent);
            return Handle(EventName.WorkflowStarted, workflowStartedEvent);
        }
        private void Raise(WorkflowStartedEvent @event)
        {
            var eventHandler = Started;
            eventHandler?.Invoke(this, new WorkflowStartedEventArgs(@event));
        }
        WorkflowAction IWorkflow.WorkflowAction(WorkflowSignaledEvent workflowSignaledEvent)
        {
            var method = _workflowEventMethods.SignalEventMethod(workflowSignaledEvent.SignalName);
            if (method == null) return workflowSignaledEvent.DefaultAction(this);
            return method.Invoke(workflowSignaledEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent)
        {
            return Handle(EventName.CancelRequest, workflowCancellationRequestedEvent);
        }
        WorkflowAction IWorkflow.WorkflowAction(RecordMarkerFailedEvent recordMarkerFailedEvent)
        {
            return Handle(EventName.RecordMarkerFailed, recordMarkerFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(WorkflowSignalFailedEvent workflowSignalFailedEvent)
        {
            return Handle(EventName.SignalFailed, workflowSignalFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(WorkflowCompletionFailedEvent workflowCompletionFailedEvent)
        {
            return Handle(EventName.CompletionFailed, workflowCompletionFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(WorkflowFailureFailedEvent workflowFailureFailedEvent)
        {
            return Handle(EventName.FailureFailed, workflowFailureFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ExternalWorkflowCancelRequestFailedEvent @event)
        {
            if(!_allWorkflowItems.HasItemFor(@event))
                return Handle(EventName.CancelRequestFailed, @event);
            var item = _allWorkflowItems.ChildWorkflowItem(@event);
            return item.CancelRequestFailedAction(@event);
        }

        WorkflowAction IWorkflow.WorkflowAction(WorkflowCancellationFailedEvent workflowCancellationFailedEvent)
        {
            return Handle(EventName.CancellationFailed, workflowCancellationFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(LambdaCompletedEvent @event)
        {
            var lambda = _allWorkflowItems.LambdaItem(@event);
            return lambda.CompletedWorkflowAction(@event);
        }

        WorkflowAction IWorkflow.WorkflowAction(LambdaFailedEvent lamdbaFailedEvent)
        {
            var lambda = _allWorkflowItems.LambdaItem(lamdbaFailedEvent);
            return lambda.FailedWorkflowAction(lamdbaFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(LambdaTimedoutEvent lambdaTimedoutEvent)
        {
            var lambda = _allWorkflowItems.LambdaItem(lambdaTimedoutEvent);
            return lambda.TimedoutWorkflowAction(lambdaTimedoutEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(LambdaSchedulingFailedEvent lamdbaSchedulingFailedEvent)
        {
            var lambda = _allWorkflowItems.LambdaItem(lamdbaSchedulingFailedEvent);
            return lambda.SchedulingFailedWorkflowAction(lamdbaSchedulingFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(LambdaStartFailedEvent lambdaStartFailedEvent)
        {
            var lambda = _allWorkflowItems.LambdaItem(lambdaStartFailedEvent);
            return lambda.StartFailedWorkflowAction(lambdaStartFailedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ChildWorkflowCompletedEvent completedEvent)
        {
            var childWorkflowItem = _allWorkflowItems.ChildWorkflowItem(completedEvent);
            return childWorkflowItem.CompletedAction(completedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ChildWorkflowFailedEvent failedEvent)
        {
            var childWorkflowItem = _allWorkflowItems.ChildWorkflowItem(failedEvent);
            return childWorkflowItem.FailedAction(failedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ChildWorkflowCancelledEvent cancelledEvent)
        {
            var childWorkflowItem = _allWorkflowItems.ChildWorkflowItem(cancelledEvent);
            return childWorkflowItem.CancelledAction(cancelledEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ChildWorkflowTerminatedEvent terminatedEvent)
        {
            var childWorkflowItem = _allWorkflowItems.ChildWorkflowItem(terminatedEvent);
            return childWorkflowItem.TerminatedAction(terminatedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ChildWorkflowTimedoutEvent timedoutEvent)
        {
            var childWorkflowItem = _allWorkflowItems.ChildWorkflowItem(timedoutEvent);
            return childWorkflowItem.TimedoutAction(timedoutEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ChildWorkflowStartFailedEvent startFailed)
        {
            var childWorkflowItem = _allWorkflowItems.ChildWorkflowItem(startFailed);
            return childWorkflowItem.StartFailed(startFailed);
        }

        WorkflowAction IWorkflow.WorkflowAction(WorkflowRestartFailedEvent failedEvent)
        {
            return Handle(EventName.RestartFailed, failedEvent);
        }

        WorkflowAction IWorkflow.WorkflowAction(ChildWorkflowStartedEvent startedEvent)
        {
            var childWorkflowItem = _allWorkflowItems.ChildWorkflowItem(startedEvent);
            return childWorkflowItem.StartedAction(startedEvent);
        }

        private WorkflowAction Handle(EventName eventName, WorkflowEvent workflowEvent)
        {
            var workflowEventMethod = _workflowEventMethods.EventMethod(eventName);
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
            return Ignore;
        }

        WorkflowAction IWorkflowDefaultActions.ResumeOnSignal(WorkflowSignaledEvent signal)
        {
            var item = WaitingItem(signal.SignalName);
            if (item == null) return Ignore;
            return item.Resume(signal);
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
        /// Schedule the activity. It reads the activity name and version from the <see cref="ActivityDescription"/> of TActivity.
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <param name="positionalName">A user defined name to differentiate same activity at multiple positions in workflow</param>
        /// <returns></returns>
        protected IFluentActivityItem ScheduleActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
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
        /// Schedule the lambda function.
        /// </summary>
        /// <param name="lambdaName">Lambda function name</param>
        /// <param name="positionalName">Positional name for lambda. It can be used to schedule same lambda multiple times in a workflow.</param>
        /// <returns></returns>
        protected IFluentLambdaItem ScheduleLambda(string lambdaName, string positionalName ="")
        {
            Ensure.NotNullAndEmpty(lambdaName, nameof(lambdaName));
            var lamdbaItem = new LambdaItem(Identity.Lambda(lambdaName, positionalName), this);
            _allWorkflowItems.Add(lamdbaItem);
            return lamdbaItem;

        }

        /// <summary>
        /// Schedule the child workflow.
        /// </summary>
        /// <param name="name">Child workflow name.</param>
        /// <param name="version">Child workflow version.</param>
        /// <param name="positionalName">Positional parameter, if any, useful in scheduling same workflow multiple times.</param>
        /// <returns></returns>
        protected IFluentChildWorkflowItem ScheduleChildWorkflow(string name, string version,
            string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            Ensure.NotNullAndEmpty(version, nameof(version));
            var childWorkflowItem = new ChildWorkflowItem(Identity.New(name, version, positionalName), this);
            _allWorkflowItems.Add(childWorkflowItem);
            return childWorkflowItem;
        }

        /// <summary>
        /// Schedule the child workflow.
        /// </summary>
        /// <typeparam name="T">Type of child workflow.</typeparam>
        /// <param name="positionalName">Positional parameter, if any, useful in scheduling same workflow multiple times.</param>
        /// <returns></returns>
        protected IFluentChildWorkflowItem ScheduleChildWorkflow<T>(string positionalName = "") where T : Workflow
        {
            var desc = WorkflowDescription.FindOn<T>();
            return ScheduleChildWorkflow(desc.Name, desc.Version, positionalName);
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
        /// Continue the scheduling the of children.
        /// </summary>
        /// <param name="workflowItemEvent"></param>
        /// <returns></returns>
        protected WorkflowAction Continue(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");

            var workflowItem = _allWorkflowItems.WorkflowItem(workflowItemEvent);
            return WorkflowAction.ContinueWorkflow(workflowItem);
        }
        /// <summary>
        /// Fail the workflow with reason and details. It will cause the workflow to be closed immediately on Amazon SWF with failed status.
        /// </summary>
        /// <param name="reason">Short reason, why workflow is failing.</param>
        /// <param name="details">Any detail about failure.</param>
        /// <returns></returns>
        protected static WorkflowAction FailWorkflow(string reason, object details)
        {
            return WorkflowAction.FailWorkflow(reason, details);
        }
        /// <summary>
        /// Complete the workflow with given result. It will cause the workflow to be closed immediately on Amazon SWF with completed status.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected static WorkflowAction CompleteWorkflow(object result)
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
        /// Close the current workflow in Amazon SWF and restart it again.
        /// </summary>
        /// <param name="defaultProperties">If it is true then default properties, configured when workflow was registered, are used otherwise it will populate the properties from current workflow.</param>
        /// <returns></returns>
        protected RestartWorkflowAction RestartWorkflow(bool defaultProperties=false)
        {
            if (defaultProperties) return WorkflowAction.RestartWorkflowWithDefaultProperties();
            IWorkflow workflow = this;
            return WorkflowAction.RestartWorkflow(workflow.WorkflowHistoryEvents);
        }
        /// <summary>
        /// Reschedule the item, associated with passed event.
        /// </summary>
        /// <param name="workflowItemEvent"></param>
        /// <returns></returns>
        protected ScheduleWorkflowItemAction Reschedule(WorkflowItemEvent workflowItemEvent)
        {
            Ensure.NotNull(workflowItemEvent, "workflowItemEvent");
            var workflowItem = _allWorkflowItems.WorkflowItem(workflowItemEvent);
            return WorkflowAction.Schedule(workflowItem);
        }
        /// <summary>
        /// Start the workflow by scheduling all the startup item. This action is different than RestartWorkflow. Later one will restart the workflow
        /// by closing the current workflow execution. While this action will not close the workflow but will only schedule the start up item.
        /// </summary>
        /// <returns></returns>
        protected WorkflowAction StartWorkflow()
        {
            return WorkflowAction.StartWorkflow(_allWorkflowItems);
        }

        /// <summary>
        /// Ignore the event and do not take any action. If called in response to a workflow item (activity, timer...) event then it will keep the branch active.
        /// </summary>
        /// <returns></returns>
        protected IgnoreWorkflowAction Ignore => WorkflowAction.Ignore(CurrentExecutingItem);
        
        /// <summary>
        /// Provides methods to jump to a children in the workflow. It will cause target item to schedule immediately (or after timeout) without checking
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
        /// Supports cancelling the the activity, timer and workflow.
        /// </summary>
        protected CancelRequest CancelRequest => new CancelRequest(_allWorkflowItems);

        /// <summary>
        /// Returns the activity for given event.
        /// </summary>
        /// <param name="activityEvent"></param>
        /// <returns></returns>
        protected IActivityItem Activity(WorkflowItemEvent activityEvent)
        {
            Ensure.NotNull(activityEvent, "@event");
            return _allWorkflowItems.Activity(activityEvent);
        }

        /// <summary>
        /// Returns the timer for given event.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        protected ITimerItem Timer(WorkflowItemEvent @event)
        {
            Ensure.NotNull(@event, "@event");
            return _allWorkflowItems.TimerItem(@event);
        }
        /// <summary>
        /// Returns the lambda for given event.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        protected ILambdaItem Lambda(WorkflowItemEvent @event)
        {
            Ensure.NotNull(@event, "@event");
            return _allWorkflowItems.LambdaItem(@event);
        }
        /// <summary>
        /// Returns child workflow for given event.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        protected IChildWorkflowItem ChildWorkflow(WorkflowItemEvent @event)
        {
            Ensure.NotNull(@event, nameof(@event));
            return _allWorkflowItems.ChildWorkflowItem(@event);
        }
        /// <summary>
        /// Returns all the children of the workflow.
        /// </summary>
        protected IEnumerable<IWorkflowItem> WorkflowItems => _allWorkflowItems.AllItems;
        /// <summary>
        /// Returns all the activities of the workflow.
        /// </summary>
        protected IEnumerable<IActivityItem> Activities => _allWorkflowItems.AllActivities;
        /// <summary>
        /// Returns all the timers of the workflow.
        /// </summary>
        protected IEnumerable<ITimerItem> Timers => _allWorkflowItems.AllTimers;

        /// <summary>
        /// Returns all the lambda functions configured in the workflow.
        /// </summary>
        protected IEnumerable<ILambdaItem> Lambdas => _allWorkflowItems.AllLambdas;
        /// <summary>
        /// Returns all the child-workflow configured in the workflow.
        /// </summary>
        protected IEnumerable<IChildWorkflowItem> ChildWorkflows => _allWorkflowItems.AllChildWorkflows;

        /// <summary>
        /// Returns the activity configured in this workflow. Throws exception if activity is not found
        /// </summary>
        /// <typeparam name="TType">Activity type</typeparam>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        protected IActivityItem Activity<TType>(string positionalName =null) where TType:Activity => Activities.First<TType>(positionalName);

        /// <summary>
        /// Returns the activity configured in this workflow. Throws exception if activity is not found.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        protected IActivityItem Activity(string name, string version, string positionalName = null) => Activities.First(name, version, positionalName);
        /// <summary>
        /// Returns the timer configured in this workflow.. Throws exception when timer is not child of this workflow.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected ITimerItem Timer(string name) => Timers.First(name);

        /// <summary>
        /// Returns the lambda configured in workflow. Throws exception when lambda is not found.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        protected ILambdaItem Lambda(string name, string positionalName ="") => Lambdas.First(name, positionalName);

        /// <summary>
        /// Returns the child workflow. Throws exception when child workflow is not found.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        protected IChildWorkflowItem ChildWorkflow(string name, string version, string positionalName="") => ChildWorkflows.First(name, version, positionalName);

        /// <summary>
        /// Returns the child workflow. Throws exception when child workflow is not found.
        /// </summary>
        /// <typeparam name="TWorkflow"></typeparam>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        protected IChildWorkflowItem ChildWorkflow<TWorkflow>(string positionalName = "") where TWorkflow : Workflow
            => ChildWorkflows.First<TWorkflow>(positionalName);

        /// <summary>
        /// Indicate if workflow history event has any active event. An active event indicates that a scheduling item (activity, timer...) is active. e.g. if an activity is just started but not finished/failed/cancelled
        /// then it is an active event.
        /// </summary>
        protected bool HasActiveEvent => ((IWorkflow)this).WorkflowHistoryEvents.HasActiveEvent();
        /// <summary>
        /// Record a marker in workflow history event. You can access all the markers by calling <see cref="AllMarkerEvents"/> API.
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
        /// Returns all marker events for this workflow.
        /// </summary>
        protected IEnumerable<MarkerRecordedEvent> AllMarkerEvents
                                        => ((IWorkflow)this).WorkflowHistoryEvents.AllMarkerRecordedEvents();

        /// <summary>
        /// Returns all the signals received by this workflow.
        /// </summary>
        protected IEnumerable<WorkflowSignaledEvent> AllSignalEvents 
                                            => ((IWorkflow)this).WorkflowHistoryEvents.AllSignalEvents();

        /// <summary>
        /// Returns all the cancellation requests made to this workflow.
        /// </summary>
        protected IEnumerable<WorkflowCancellationRequestedEvent> AllCancellationRequestedEvents
                                    => ((IWorkflow)this).WorkflowHistoryEvents.AllWorkflowCancellationRequestedEvents();

        /// <summary>
        /// Returns the event id of latest history event.
        /// </summary>
        protected long LatestEventId
            => ((IWorkflow) this).WorkflowHistoryEvents.LatestEventId;
        /// <summary>
        /// Returns the workflow id.
        /// </summary>
        protected string Id
        => ((IWorkflow)this).WorkflowHistoryEvents.WorkflowId;
        /// <summary>
        /// Return the workflow run id.
        /// </summary>
        protected string RunId
            => ((IWorkflow)this).WorkflowHistoryEvents.WorkflowRunId;

        /// <summary>
        /// APIs to send signal to other workflow (child/non-child).
        /// </summary>
        /// <param name="signalName"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected Signal Signal(string signalName, object input)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            return new Signal(signalName, input, this);
        }
        /// <summary>
        /// Represent the incoming signal.
        /// </summary>
        /// <param name="signalName"></param>
        /// <returns></returns>
        protected InwardSignal Signal(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            return new InwardSignal(signalName, this);
        }
        /// <summary>
        /// Return workflow input as dynamic object. If workflow input is JSON data then you can directly access its properties. e.g. Input.OrderId.
        /// </summary>
        protected dynamic Input => StartedEvent.Input.AsDynamic();

        /// <summary>
        /// Returns the workflow input as TType object.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        protected TType InputAs<TType>()
        {
            return StartedEvent.Input.As<TType>();
        }

        /// <summary>
        /// Returns workflow started event.
        /// </summary>
        protected WorkflowStartedEvent StartedEvent
        {
            get
            {
                IWorkflow workflow = this;
                return workflow.WorkflowHistoryEvents.WorkflowStartedEvent();
            }
        }

        /// <summary>
        /// Returns workflow item waiting for given signal. Returns null if no workflow item is waiting for given signal. 
        /// </summary>
        /// <param name="signalName">Signal name</param>
        /// <returns></returns>
        protected IWorkflowItem WaitingItem(string signalName)=> WaitingItems(signalName).FirstOrDefault();

        /// <summary>
        /// Returns all the workflow items waiting for the given signal. It returns multiple workflow items when more than one workflow item are waiting for the same signal in different branches.
        /// </summary>
        /// <param name="signalName"></param>
        /// <returns></returns>
        protected IEnumerable<IWorkflowItem> WaitingItems(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            IWorkflow workflow = this;
            return workflow.WaitForSignalsEvents.WaitingItems(_allWorkflowItems.AllItems.ToArray(), signalName);
        }

        IEnumerable<WorkflowItem> IWorkflow.GetChildernOf(WorkflowItem workflowItem)=> _allWorkflowItems.Childeren(workflowItem);
        WorkflowItem IWorkflow.WorkflowItem(Identity identity)=> _allWorkflowItems.WorkflowItem(identity);
        WaitForSignalsEvent IWorkflow.TimedoutEventTriggerBy(WorkflowEvent workflowEvent)
        {
            IWorkflow workflow = this;
            return workflow.WaitForSignalsEvents.FirstOrDefault(workflowEvent.EventId);
        }

        ChildWorkflowItem IWorkflow.ChildWorkflowItem(Identity identity) =>
            _allWorkflowItems.ChildWorkflowItem(identity);
       

        private WorkflowItem CurrentExecutingItem
        {
            get
            {
                var workflowItemEvent = _currentWorkflowHistoryEvents.CurrentExecutingEvent as WorkflowItemEvent;
                if (workflowItemEvent != null)
                    return _allWorkflowItems.WorkflowItem(workflowItemEvent);

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

        IEnumerable<WaitForSignalsEvent> IWorkflow.WaitForSignalsEvents
        {
            get
            {
                IWorkflow workflow = this;
                return workflow.WorkflowHistoryEvents.WaitForSignalsEvents()
                    .Concat(_generatedActions.WaitForSignalsEvents());
            }
        }

        WorkflowEvent IWorkflow.CurrentlyExecutingEvent => _currentWorkflowHistoryEvents.CurrentExecutingEvent;

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
        /// <summary>
        /// This method is called when the workflow is about to be completed. You can override this method to provide custom <see cref="WorkflowAction"/> during completion.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual WorkflowAction DuringCompletion(string result)
        {
            return CompleteWorkflow(result);
        }
        /// <summary>
        /// This method is called when the workflow is about to failed. You can override this method to provide custom <see cref="WorkflowAction"/> during failure.
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        protected virtual WorkflowAction DuringFailure(string reason, string detail)
        {
            return FailWorkflow(reason, detail);
        }
        /// <summary>
        /// This method is called when the workflow is about to be cancelled. You can override this method to provide custom <see cref="WorkflowAction"/> during cancellation.
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        protected virtual WorkflowAction DuringCancellation(string details)
        {
            return CancelWorkflow(details);
        }
        /// <summary>
        /// This method is called before the workflow interprets the new events.
        /// </summary>
        protected virtual void BeforeExecution()
        {
        }
        /// <summary>
        /// This method is called after workflow has generated the decisions to sent to Amazon SWF.
        /// </summary>
        protected virtual void AfterExecution()
        {
        }
       
        internal IEnumerable<WorkflowDecision> Decisions(IWorkflowHistoryEvents historyEvents)
        {
            _generatedActions = new WorkflowActions();
            var decisions = new List<WorkflowDecision>();
            try
            {
                _currentWorkflowHistoryEvents = historyEvents;
                BeforeExecution();
                foreach (var workflowEvent in historyEvents.NewEvents())
                {
                    _currentWorkflowHistoryEvents.CurrentExecutingEvent = workflowEvent;
                    var action = workflowEvent.Interpret(this) ?? WorkflowAction.Empty;
                    _generatedActions.Add(action);
                    decisions.AddRange(action.Decisions(this));
                    _currentWorkflowHistoryEvents.CurrentExecutingEvent = null;
                }

                return decisions.Distinct().CompatibleDecisions(this).Where(d => d != WorkflowDecision.Empty).ToArray();
            }
            finally
            {   
                //Following line may not be required because clean up has handled before it comes here.
                _allWorkflowItems.ResetContinueItems();

                AfterExecution();
                _currentWorkflowHistoryEvents.CurrentExecutingEvent = null;
                _currentWorkflowHistoryEvents = null;

            }
        }
    }
}