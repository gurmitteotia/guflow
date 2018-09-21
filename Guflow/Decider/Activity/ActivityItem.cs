// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal sealed class ActivityItem : WorkflowItem, IFluentActivityItem, IActivityItem, ITimer, IActivity
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
        private Func<IActivityItem, WorkflowAction> _onFalseAction;
        private Func<IActivityItem, int?> _priorityFunc;
        private Func<IActivityItem, ActivityTimeouts> _timeoutsFunc;
        private readonly TimerItem _rescheduleTimer;
        internal ActivityItem(Identity identity, IWorkflow workflow)
            : base(identity, workflow)
        {
            _onCompletionAction = c => c.DefaultAction(workflow);
            _onFailedAction = c => c.DefaultAction(workflow);
            _onTimedoutAction = t => t.DefaultAction(workflow);
            _onCancelledAction = c => c.DefaultAction(workflow);
            _onCancellationFailedAction = c => c.DefaultAction(workflow);
            _onFailedSchedulingAction = c => c.DefaultAction(workflow);
            _inputFunc = a => WorkflowHistoryEvents.WorkflowStartedEvent().Input;
            _taskListFunc = a => null;
            _whenFunc = a => true;
            _onFalseAction = _ =>IsStartupItem() ? WorkflowAction.Empty : new TriggerActions(this).FirstJoint();
            _priorityFunc = a => null;
            _timeoutsFunc = a => new ActivityTimeouts();
            _rescheduleTimer = TimerItem.Reschedule(this, identity, workflow);
        }

        public override WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false)
        {
            var latestActivityEvent = WorkflowHistoryEvents.LastActivityEvent(this);
            WorkflowItemEvent latestTimerEvent = null;
            if (includeRescheduleTimerEvents)
                latestTimerEvent = WorkflowHistoryEvents.LastTimerEvent(_rescheduleTimer, true);

            if (latestActivityEvent > latestTimerEvent)
                return latestActivityEvent;

            return latestTimerEvent;
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false)
        {
            var activityEvents = WorkflowHistoryEvents.AllActivityEvents(this);
            var timerEvents = Enumerable.Empty<WorkflowItemEvent>();
            if(includeRescheduleTimerEvents)
                timerEvents = WorkflowHistoryEvents.AllTimerEvents(_rescheduleTimer, true);
            return activityEvents.Concat(timerEvents).OrderByDescending(i => i, WorkflowEvent.IdComparer);
        }

        public string Version => Identity.Version;

        public string PositionalName => Identity.PositionalName;

        public IFluentActivityItem AfterActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "activityName");
            Ensure.NotNullAndEmpty(version, "activityVersion");

            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public IFluentActivityItem AfterTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "timerName");

            AddParent(Identity.Timer(name));
            return this;
        }
        public IFluentActivityItem AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            return AfterActivity(description.Name, description.Version, positionalName);
        }

        public IFluentActivityItem AfterLambda(string name, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            AddParent(Identity.Lambda(name, positionalName));
            return this;
        }

        public IFluentActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");
            _onCompletionAction = action;
            return this;
        }
        public IFluentActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");
            _onFailedAction = action;
            return this;
        }
        public IFluentActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");
            _onTimedoutAction = action;
            return this;
        }

        public IFluentActivityItem OnSchedulingFailed(Func<ActivitySchedulingFailedEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");
            _onFailedSchedulingAction = action;
            return this;
        }

        public IFluentActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");
            _onCancelledAction = action;
            return this;
        }

        public IFluentActivityItem OnCancellationFailed(Func<ActivityCancellationFailedEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");

            _onCancellationFailedAction = action;
            return this;
        }
        public IFluentActivityItem WithInput(Func<IActivityItem, object> data)
        {
            Ensure.NotNull(data, "data");
            _inputFunc = data;
            return this;
        }
        public IFluentActivityItem OnTaskList(Func<IActivityItem, string> name)
        {
            Ensure.NotNull(name, "name");
            _taskListFunc = name;
            return this;
        }
        public IFluentActivityItem When(Func<IActivityItem, bool> @true)
        {
            Ensure.NotNull(@true, "@true");
            _whenFunc = @true;
            return this;
        }

        public IFluentActivityItem When(Func<IActivityItem, bool> @true, Func<IActivityItem,WorkflowAction> falseAction)
        {
            Ensure.NotNull(falseAction,nameof(falseAction));
            _onFalseAction = falseAction;
            return When(@true);
        }

        public IFluentActivityItem WithPriority(Func<IActivityItem, int?> number)
        {
            Ensure.NotNull(number, "number");

            _priorityFunc = number;
            return this;
        }

        public IFluentActivityItem WithTimeouts(Func<IActivityItem, ActivityTimeouts> timeouts)
        {
            Ensure.NotNull(timeouts, "timeouts");
            _timeoutsFunc = timeouts;
            return this;
        }

        public IFluentTimerItem RescheduleTimer => _rescheduleTimer;

        WorkflowAction ITimer.Fired(TimerFiredEvent timerFiredEvent)
        {
            ITimer timer = _rescheduleTimer;
            return timer.Fired(timerFiredEvent);
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
        public override IEnumerable<WorkflowDecision> GetScheduleDecisions()
        {
            if (!_whenFunc(this))
                return _onFalseAction(this).Decisions();

            var scheduleActivityDecision = new ScheduleActivityDecision(Identity);
            scheduleActivityDecision.Input = _inputFunc(this).ToAwsString();
            scheduleActivityDecision.TaskList = _taskListFunc(this);
            scheduleActivityDecision.TaskPriority = _priorityFunc(this);
            scheduleActivityDecision.Timeouts = _timeoutsFunc(this);
            return new []{scheduleActivityDecision};
        }

        public override IEnumerable<WorkflowDecision> GetRescheduleDecisions(TimeSpan timeout)
        {
            _rescheduleTimer.FireAfter(timeout);
            return _rescheduleTimer.GetScheduleDecisions();
        }

        public override IEnumerable<WorkflowDecision> GetCancelDecisions()
        {
            var lastEvent = LastEvent(true);
            var latestTimerEvent = WorkflowHistoryEvents.LastTimerEvent(_rescheduleTimer, true);
            if (latestTimerEvent != null && lastEvent == latestTimerEvent)
                return _rescheduleTimer.GetCancelDecisions();

            return new []{new CancelActivityDecision(Identity)};
        }

    }
}