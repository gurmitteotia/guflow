using System;

namespace Guflow
{
    internal sealed class ActivityItem : WorkflowItem, IFluentActivityItem, IActivityItem
    {
        private Func<ActivityCompletedEvent, WorkflowAction> _onCompletionAction;
        private Func<ActivityFailedEvent, WorkflowAction> _onFailedAction;
        private Func<ActivityTimedoutEvent, WorkflowAction> _onTimedoutAction;
        private Func<ActivityCancelledEvent, WorkflowAction> _onCancelledAction;
        private Func<ActivityCancellationFailedEvent, WorkflowAction> _onCancellationFailedAction;
        private Func<ActivitySchedulingFailedEvent, WorkflowAction> _onFailedSchedulingAction;
        private Func<IActivityItem, string> _inputFunc;
        private Func<IActivityItem, string> _taskListFunc;
        private Func<IActivityItem, bool> _whenFunc;
        private Func<IActivityItem, int?> _priorityFunc;
        private Func<IActivityItem, ScheduleActivityTimeouts> _timeoutsFunc;

        internal ActivityItem(Identity identity, IWorkflowItems workflowItems)
            : base(identity, workflowItems)
        {
            _onCompletionAction = c => WorkflowAction.ContinueWorkflow(this);
            _onFailedAction = c => WorkflowAction.FailWorkflow(c.Reason, c.Detail);
            _onTimedoutAction = t => WorkflowAction.FailWorkflow(t.TimeoutType, t.Details);
            _onCancelledAction = c => WorkflowAction.CancelWorkflow(c.Details);
            _onCancellationFailedAction = c => WorkflowAction.FailWorkflow("ACTIVITY_CANCELLATION_FAILED", c.Cause);
            _onFailedSchedulingAction = c => WorkflowAction.FailWorkflow("ACTIVITY_SCHEDULING_FAILED", c.Cause);
            _inputFunc = a => WorkflowHistoryEvents.WorkflowStartedEvent().Input;
            _taskListFunc = a => null;
            _whenFunc = a => true;
            _priorityFunc = a => null;
            _timeoutsFunc = a => new ScheduleActivityTimeouts();
        }

        public WorkflowItemEvent LatestEvent
        {
            get { return WorkflowHistoryEvents.LatestEventFor(this); }
        }
        public string Version
        {
            get { return Identity.Version; }
        }

        public string PositionalName
        {
            get { return Identity.PositionalName; }
        }

        public IFluentActivityItem DependsOn(string name, string version, string positionalName = "")
        {
            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public IFluentActivityItem DependsOn(string timerName)
        {
            AddParent(Identity.Timer(timerName));
            return this;
        }
    
        public IFluentActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> onCompletionFunc)
        {
            _onCompletionAction = onCompletionFunc;
            return this;
        }
        public IFluentActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> onFailureFunc)
        {
            _onFailedAction = onFailureFunc;
            return this;
        }
        public IFluentActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> onTimedoutFunc)
        {
            _onTimedoutAction = onTimedoutFunc;
            return this;
        }

        public IFluentActivityItem OnFailedScheduling(Func<ActivitySchedulingFailedEvent, WorkflowAction> onFailedSchedulingAction)
        {
            _onFailedSchedulingAction = onFailedSchedulingAction;
            return this;
        }

        public IFluentActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> onCancelledFunc)
        {
            _onCancelledAction = onCancelledFunc;
            return this;
        }

        public IFluentActivityItem OnFailedCancellation(Func<ActivityCancellationFailedEvent, WorkflowAction> onFailedCancellationAction)
        {
            _onCancellationFailedAction = onFailedCancellationAction;
            return this;
        }
        public IFluentActivityItem WithInput(Func<IActivityItem, string> inputFunc)
        {
            _inputFunc = inputFunc;
            return this;
        }
        public IFluentActivityItem OnTaskList(Func<IActivityItem, string> taskListFunc)
        {
            _taskListFunc = taskListFunc;
            return this;
        }
        public IFluentActivityItem When(Func<IActivityItem, bool> whenFunc)
        {
            _whenFunc = whenFunc;
            return this;
        }
        public IFluentActivityItem WithPriority(Func<IActivityItem, int?> priorityFunc)
        {
            _priorityFunc = priorityFunc;
            return this;
        }

        public IFluentActivityItem WithTimeouts(Func<IActivityItem, ScheduleActivityTimeouts> timeoutsFunc)
        {
            _timeoutsFunc = timeoutsFunc;
            return this;
        }

        public IFluentTimerItem RescheduleTimer
        {
            get { return RescheduleTimerItem; }
        }

        internal override WorkflowDecision GetScheduleDecision()
        {
            if(!_whenFunc(this))
                return WorkflowDecision.Empty;

            var scheduleActivityDecision = new ScheduleActivityDecision(Identity);
            scheduleActivityDecision.UseInputFunc(()=>_inputFunc(this));
            scheduleActivityDecision.TaskList = _taskListFunc(this);
            scheduleActivityDecision.TaskPriority = _priorityFunc(this);
            scheduleActivityDecision.Timeouts = _timeoutsFunc(this);
            return scheduleActivityDecision;
        }

        internal override WorkflowDecision GetCancelDecision()
        {
            return new CancelActivityDecision(Identity);
        }

        internal override WorkflowAction TimerFired(TimerFiredEvent timerFiredEvent)
        {
            return RescheduleTimerItem.TimerFired(timerFiredEvent);
        }

        internal override WorkflowAction TimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            return RescheduleTimerItem.TimerCancelled(timerCancelledEvent);
        }

        internal override WorkflowAction TimerStartFailed(TimerStartFailedEvent timerStartFailedEvent)
        {
            return RescheduleTimerItem.TimerStartFailed(timerStartFailedEvent);
        }

        internal WorkflowAction Completed(ActivityCompletedEvent activityCompletedEvent)
        {
            return _onCompletionAction(activityCompletedEvent);
        }
        protected override bool IsProcessed()
        {
            var activity = LatestEvent;
            return activity != null && !activity.IsActive ;
        }
        internal WorkflowAction Failed(ActivityFailedEvent activityFailedEvent)
        {
            return _onFailedAction(activityFailedEvent);
        }
        internal WorkflowAction Timedout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            return _onTimedoutAction(activityTimedoutEvent);
        }
        internal WorkflowAction Cancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            return _onCancelledAction(activityCancelledEvent);
        }
        internal WorkflowAction CancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            return _onCancellationFailedAction(activityCancellationFailedEvent);
        }
        internal WorkflowAction SchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent)
        {
            return _onFailedSchedulingAction(activitySchedulingFailedEvent);
        }
    }
}