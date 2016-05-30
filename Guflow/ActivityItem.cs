using System;

namespace Guflow
{
    public sealed class ActivityItem : WorkflowItem
    {
        private Func<ActivityCompletedEvent, WorkflowAction> _onCompletionAction;
        private Func<ActivityFailedEvent, WorkflowAction> _onFailedAction;
        private Func<ActivityTimedoutEvent, WorkflowAction> _onTimedoutAction;
        private Func<ActivityCancelledEvent, WorkflowAction> _onCancelledAction;
        private Func<ActivityCancellationFailedEvent, WorkflowAction> _onCancellationFailedAction;
        private Func<ActivityItem, string> _inputFunc;
        private Func<ActivityItem, string> _taskListFunc;
        private Func<ActivityItem, bool> _whenFunc;
        private Func<ActivityItem, int?> _priorityFunc;
        private Func<ActivityItem, ScheduleActivityTimeouts> _timeoutsFunc; 
        internal ActivityItem(string name, string version, string positionalName, IWorkflowItems workflowItems)
            : base(Identity.New(name, version, positionalName), workflowItems)
        {
            _onCompletionAction = c => WorkflowAction.ContinueWorkflow(this);
            _onFailedAction = c => WorkflowAction.FailWorkflow(c.Reason, c.Detail);
            _onTimedoutAction = t => WorkflowAction.FailWorkflow(t.TimeoutType, t.Details);
            _onCancelledAction = c => WorkflowAction.CancelWorkflow(c.Details);
            _onCancellationFailedAction = c => WorkflowAction.FailWorkflow("ACTIVITY_CANCELLATION_FAILED", c.Cause);
            _inputFunc = a => WorkflowHistoryEvents.WorkflowStartedEvent().Input;
            _taskListFunc = a => null;
            _whenFunc = a => true;
            _priorityFunc = a => null;
            _timeoutsFunc = a =>new ScheduleActivityTimeouts();
        }

        public ActivityItem DependsOn(string name, string version, string positionalName = "")
        {
            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public ActivityItem DependsOn(string timerName)
        {
            AddParent(Identity.Timer(timerName));
            return this;
        }
        public ActivityEvent LatestEvent
        {
            get { return WorkflowHistoryEvents.LatestActivityEventFor(this); }
        }

        public ActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> onCompletionFunc)
        {
            _onCompletionAction = onCompletionFunc;
            return this;
        }
        public ActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> onFailureFunc)
        {
            _onFailedAction = onFailureFunc;
            return this;
        }
        public ActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> onTimedoutFunc)
        {
            _onTimedoutAction = onTimedoutFunc;
            return this;
        }
        public ActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> onCancelledFunc)
        {
            _onCancelledAction = onCancelledFunc;
            return this;
        }

        public ActivityItem OnTimerCancelled(Func<TimerCancelledEvent, WorkflowAction> onTimerCancelledAction)
        {
            OnTimerCancelledAction = onTimerCancelledAction;
            return this;
        }
        public ActivityItem OnFailedCancellation(Func<ActivityCancellationFailedEvent, WorkflowAction> onFailedCancellationAction)
        {
            _onCancellationFailedAction = onFailedCancellationAction;
            return this;
        }
        public ActivityItem WithInput(Func<ActivityItem, string> inputFunc)
        {
            _inputFunc = inputFunc;
            return this;
        }
        public ActivityItem OnTaskList(Func<ActivityItem, string> taskListFunc)
        {
            _taskListFunc = taskListFunc;
            return this;
        }
        public ActivityItem When(Func<ActivityItem, bool> whenFunc)
        {
            _whenFunc = whenFunc;
            return this;
        }
        public ActivityItem WithPriority(Func<ActivityItem, int?> priorityFunc)
        {
            _priorityFunc = priorityFunc;
            return this;
        }

        public ActivityItem WithTimeouts(Func<ActivityItem, ScheduleActivityTimeouts> timeoutsFunc)
        {
            _timeoutsFunc = timeoutsFunc;
            return this;
        }
        internal override WorkflowDecision GetScheduleDecision()
        {
            if(!_whenFunc(this))
                return WorkflowDecision.Empty;

            var scheduleActivityDecision = new ScheduleActivityDecision(Identity);
            scheduleActivityDecision.Input = _inputFunc(this);
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
            return WorkflowAction.Schedule(this);
        }

        internal WorkflowAction Completed(ActivityCompletedEvent activityCompletedEvent)
        {
            return _onCompletionAction(activityCompletedEvent);
        }
        protected override bool IsProcessed()
        {
            var activity = LatestEvent;
            return activity != null;
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
    }
}