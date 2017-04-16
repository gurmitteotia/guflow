using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal sealed class ActivityItem : WorkflowItem, IFluentActivityItem, IActivityItem, ITimer, IActivity, ISchedulableItem
    {
        private Func<ActivityCompletedEvent, WorkflowAction> _onCompletionAction;
        private Func<ActivityFailedEvent, WorkflowAction> _onFailedAction;
        private Func<ActivityTimedoutEvent, WorkflowAction> _onTimedoutAction;
        private Func<ActivityCancelledEvent, WorkflowAction> _onCancelledAction;
        private Func<ActivityCancellationFailedEvent, WorkflowAction> _onCancellationFailedAction;
        private Func<ActivitySchedulingFailedEvent, WorkflowAction> _onFailedSchedulingAction;
        private Func<IActivityItem, object> _inputFunc;
        private Func<IActivityItem, string> _taskListFunc;
        private Func<IActivityItem, bool> _whenFunc;
        private Func<IActivityItem, int?> _priorityFunc;
        private Func<IActivityItem, ScheduleActivityTimeouts> _timeoutsFunc;
        private readonly TimerItem _rescheduleTimer;
        internal ActivityItem(Identity identity, IWorkflow workflow)
            : base(identity, workflow)
        {
            _onCompletionAction = c => WorkflowAction.ContinueWorkflow(this);
            _onFailedAction = c => WorkflowAction.FailWorkflow(c.Reason, c.Detail);
            _onTimedoutAction = t => WorkflowAction.FailWorkflow(t.TimeoutType, t.Details);
            _onCancelledAction = c => WorkflowAction.CancelWorkflow(c.Details);
            _onCancellationFailedAction = c => WorkflowAction.FailWorkflow("ACTIVITY_CANCELLATION_FAILED", c.Cause);
            _onFailedSchedulingAction = c => WorkflowAction.FailWorkflow("ACTIVITY_SCHEDULING_FAILED", c.Cause);
            _inputFunc = a => WorkflowEvents.WorkflowStartedEvent().Input;
            _taskListFunc = a => null;
            _whenFunc = a => true;
            _priorityFunc = a => null;
            _timeoutsFunc = a => new ScheduleActivityTimeouts();
            SchedulableItem = this;
            _rescheduleTimer = TimerItem.Reschedule(this, identity, workflow);
        }

        public override WorkflowItemEvent LastEvent
        {
            get
            {
                var latestActivityEvent = WorkflowEvents.LastActivityEventFor(this);
                var latestTimerEvent = WorkflowEvents.LastTimerEventFor(_rescheduleTimer);
                if (latestActivityEvent > latestTimerEvent)
                    return latestActivityEvent;

                return latestTimerEvent;
            }
        }
        public ActivityCompletedEvent LastCompletedEvent
        {
            get { return WorkflowEvents.LastCompletedEventFor(this); }
        }

        public ActivityFailedEvent LastFailedEvent
        {
            get { return WorkflowEvents.LastFailedEventFor(this); }
        }

        public ActivityTimedoutEvent LastTimedoutEvent
        {
            get { return WorkflowEvents.LastTimedoutEventFor(this); }
        }

        public ActivityCancelledEvent LastCancelledEvent
        {
            get { return WorkflowEvents.LastCancelledEventFor(this); }
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents
        {
            get
            {
                var activityEvents = WorkflowEvents.AllActivityEventsFor(this);
                var timerEvents = WorkflowEvents.AllTimerEventsFor(_rescheduleTimer);
                return activityEvents.Concat(timerEvents).OrderByDescending(i => i, WorkflowEvent.IdComparer);
            }
        }

        public string Version
        {
            get { return Identity.Version; }
        }

        public string PositionalName
        {
            get { return Identity.PositionalName; }
        }

        public IFluentActivityItem After(string activityName, string activityVersion, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(activityName, "activityName");
            Ensure.NotNullAndEmpty(activityVersion, "activityVersion");

            AddParent(Identity.New(activityName, activityVersion, positionalName));
            return this;
        }

        public IFluentActivityItem After(string timerName)
        {
            Ensure.NotNullAndEmpty(timerName, "timerName");

            AddParent(Identity.Timer(timerName));
            return this;
        }

        public IFluentActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> onCompletionFunc)
        {
            Ensure.NotNull(onCompletionFunc, "onCompletionFunc");
            _onCompletionAction = onCompletionFunc;
            return this;
        }
        public IFluentActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> onFailureFunc)
        {
            Ensure.NotNull(onFailureFunc, "onFailureFunc");
            _onFailedAction = onFailureFunc;
            return this;
        }
        public IFluentActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> onTimedoutFunc)
        {
            Ensure.NotNull(onTimedoutFunc, "onTimedoutFunc");
            _onTimedoutAction = onTimedoutFunc;
            return this;
        }

        public IFluentActivityItem OnFailedScheduling(Func<ActivitySchedulingFailedEvent, WorkflowAction> onFailedSchedulingAction)
        {
            Ensure.NotNull(onFailedSchedulingAction, "onFailedSchedulingAction");
            _onFailedSchedulingAction = onFailedSchedulingAction;
            return this;
        }

        public IFluentActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> onCancelledFunc)
        {
            Ensure.NotNull(onCancelledFunc, "onCancelledFunc");
            _onCancelledAction = onCancelledFunc;
            return this;
        }

        public IFluentActivityItem OnFailedCancellation(Func<ActivityCancellationFailedEvent, WorkflowAction> onFailedCancellationFunc)
        {
            Ensure.NotNull(onFailedCancellationFunc, "onFailedCancellationFunc");

            _onCancellationFailedAction = onFailedCancellationFunc;
            return this;
        }
        public IFluentActivityItem WithInput(Func<IActivityItem, object> inputFunc)
        {
            Ensure.NotNull(inputFunc, "inputFunc");
            _inputFunc = inputFunc;
            return this;
        }
        public IFluentActivityItem OnTaskList(Func<IActivityItem, string> taskListFunc)
        {
            Ensure.NotNull(taskListFunc, "taskListFunc");
            _taskListFunc = taskListFunc;
            return this;
        }
        public IFluentActivityItem When(Func<IActivityItem, bool> whenFunc)
        {
            Ensure.NotNull(whenFunc, "whenFunc");

            _whenFunc = whenFunc;
            return this;
        }
        public IFluentActivityItem WithPriority(Func<IActivityItem, int?> priorityFunc)
        {
            Ensure.NotNull(priorityFunc, "priorityFunc");

            _priorityFunc = priorityFunc;
            return this;
        }

        public IFluentActivityItem WithTimeouts(Func<IActivityItem, ScheduleActivityTimeouts> timeoutsFunc)
        {
            Ensure.NotNull(timeoutsFunc, "timeoutsFunc");
            _timeoutsFunc = timeoutsFunc;
            return this;
        }

        public IFluentTimerItem RescheduleTimer
        {
            get { return _rescheduleTimer; }
        }
        WorkflowAction ITimer.Fired(TimerFiredEvent timerFiredEvent)
        {
            ITimer timer = _rescheduleTimer;
            return timer.Fired(timerFiredEvent);
        }
        WorkflowAction ITimer.Cancelled(TimerCancelledEvent timerCancelledEvent)
        {
            ITimer timer = _rescheduleTimer;
            return timer.Cancelled(timerCancelledEvent);
        }
        WorkflowAction ITimer.StartFailed(TimerStartFailedEvent timerStartFailedEvent)
        {
            ITimer timer = _rescheduleTimer;
            return timer.StartFailed(timerStartFailedEvent);
        }
        WorkflowAction ITimer.CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            ITimer timer = _rescheduleTimer;
            return timer.CancellationFailed(timerCancellationFailedEvent);
        }

        WorkflowAction IActivity.Completed(ActivityCompletedEvent activityCompletedEvent)
        {
            return _onCompletionAction(activityCompletedEvent);
        }

        WorkflowAction IActivity.Failed(ActivityFailedEvent activityFailedEvent)
        {
            return _onFailedAction(activityFailedEvent);
        }

        WorkflowAction IActivity.Timedout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            return _onTimedoutAction(activityTimedoutEvent);
        }

        WorkflowAction IActivity.Cancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            return _onCancelledAction(activityCancelledEvent);
        }

        WorkflowAction IActivity.CancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            return _onCancellationFailedAction(activityCancellationFailedEvent);
        }

        WorkflowAction IActivity.SchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent)
        {
            return _onFailedSchedulingAction(activitySchedulingFailedEvent);
        }
        private string GetActivityInput()
        {
            var inputObject = _inputFunc(this);
            return inputObject.ToAwsString();
        }

        WorkflowDecision ISchedulableItem.ScheduleDecision(Identity identity)
        {
            if (!_whenFunc(this))
                return WorkflowDecision.Empty;

            var scheduleActivityDecision = new ScheduleActivityDecision(identity);
            scheduleActivityDecision.UseInputFunc(GetActivityInput);
            scheduleActivityDecision.TaskList = _taskListFunc(this);
            scheduleActivityDecision.TaskPriority = _priorityFunc(this);
            scheduleActivityDecision.Timeouts = _timeoutsFunc(this);
            return scheduleActivityDecision;
        }

        WorkflowDecision ISchedulableItem.RescheduleDecision(Identity identity, TimeSpan afterTimeout)
        {
            _rescheduleTimer.FireAfter(afterTimeout);
            return _rescheduleTimer.GetScheduleDecision();
        }

        WorkflowDecision ISchedulableItem.CancelDecision(Identity identity)
        {
            var lastEvent = LastEvent;
            var latestTimerEvent = WorkflowEvents.LastTimerEventFor(_rescheduleTimer);
            if (latestTimerEvent != WorkflowItemEvent.NotFound && lastEvent == latestTimerEvent)
                return _rescheduleTimer.GetCancelDecision();

            return new CancelActivityDecision(identity);
        }
    }
}