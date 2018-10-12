// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal sealed class TimerItem : WorkflowItem, IFluentTimerItem, ITimerItem, ITimer
    {
        private TimeSpan _fireAfter= new TimeSpan();
        private Func<ITimerItem, TimeSpan> _fireAfterFunc;
        private Func<TimerFiredEvent, WorkflowAction> _firedAction;
        private Func<TimerCancellationFailedEvent, WorkflowAction> _cancellationFailedAction;
        private Func<TimerStartFailedEvent, WorkflowAction> _startFailureAction;
        private Func<TimerItem, bool> _canSchedule;
        private Func<ITimerItem, WorkflowAction> _falseAction;
        private Func<ITimerItem, WorkflowAction> _timerCancelAction;
        private TimerItem _rescheduleTimer;

        private bool _invokedTimerCancelAction = false;// to avoid recurssion.

        private TimerItem(Identity identity, IWorkflow workflow)
                     : base(identity, workflow)
        {
            _canSchedule = t => true;
            _falseAction = t=>new TriggerActions(this).FirstJoint();
            _timerCancelAction =_=>WorkflowAction.Empty;
            _fireAfterFunc = _ => _fireAfter;
        }

        public static TimerItem Reschedule(WorkflowItem ownerItem, Identity identity, IWorkflow workflow)
        {
            var timerItem = new TimerItem(identity, workflow);
            timerItem._rescheduleTimer = timerItem;
            timerItem.OnStartFailed(e => WorkflowAction.FailWorkflow("RESCHEDULE_TIMER_START_FAILED", e.Cause));
            timerItem.OnCancellationFailed(e => WorkflowAction.FailWorkflow("RESCHEDULE_TIMER_CANCELLATION_FAILED", e.Cause));
            timerItem.OnFired(e => WorkflowAction.Schedule(ownerItem));
            return timerItem;
        }
        public static TimerItem New(Identity identity, IWorkflow workflow)
        {
            var timerItem = new TimerItem(identity, workflow);
            timerItem._rescheduleTimer = Reschedule(timerItem, identity, workflow);
            timerItem.OnStartFailed(e => e.DefaultAction(workflow));
            timerItem.OnCancellationFailed(e => e.DefaultAction(workflow));
            timerItem.OnFired(e => e.DefaultAction(workflow));
            return timerItem;
        }
        public override WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false)
            => WorkflowHistoryEvents.LastTimerEvent(this, includeRescheduleTimerEvents);

        public override IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false) 
            => WorkflowHistoryEvents.AllTimerEvents(this, includeRescheduleTimerEvents);

        public IFluentTimerItem FireAfter(TimeSpan time)
        {
            _fireAfter = time;
            return this;
        }

        public IFluentTimerItem FireAfter(Func<ITimerItem, TimeSpan> time)
        {
            Ensure.NotNull(time, "time");
            _fireAfterFunc = time;
            return this;
        }

        public IFluentTimerItem When(Func<ITimerItem, bool> @true)
        {
            Ensure.NotNull(@true,"@true");

            _canSchedule = @true;
            return this;
        }

        public IFluentTimerItem When(Func<ITimerItem, bool> @true, Func<ITimerItem, WorkflowAction> falseAction)
        {
            Ensure.NotNull(falseAction,nameof(falseAction));
            _falseAction = falseAction;
            return When(@true);
        }

        public IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");

            _firedAction = action;
            return this;
        }
        public IFluentTimerItem AfterTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "timerName");
            AddParent(Identity.Timer(name));
            return this;
        }

        public IFluentTimerItem AfterActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "activityName");
            Ensure.NotNullAndEmpty(version, "activityVersion");

            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public IFluentTimerItem AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            return AfterActivity(description.Name, description.Version, positionalName);
        }

        public IFluentTimerItem AfterLambda(string name, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            AddParent(Identity.Lambda(name, positionalName));
            return this;
        }

        public IFluentTimerItem AfterChildWorkflow(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            Ensure.NotNullAndEmpty(version, nameof(version));
            AddParent(Identity.New(name,version, positionalName));
            return this;
        }

        public IFluentTimerItem AfterChildWorkflow<TWorkflow>(string positionalName) where TWorkflow : Workflow
        {
            var desc = WorkflowDescription.FindOn<TWorkflow>();
            return AfterChildWorkflow(desc.Name, desc.Version, positionalName);
        }

        public IFluentTimerItem OnCancellationFailed(Func<TimerCancellationFailedEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");

            _cancellationFailedAction = action;
            return this;
        }

        public IFluentTimerItem OnStartFailed(Func<TimerStartFailedEvent, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");
            _startFailureAction = action;
            return this;
        }

        public IFluentTimerItem OnCancel(Func<ITimerItem, WorkflowAction> action)
        {
            Ensure.NotNull(action, "action");
            _timerCancelAction = action;
            return this;
        }

        WorkflowAction ITimer.Fired(TimerFiredEvent timerFiredEvent)
        {
            if (timerFiredEvent.IsARescheduledTimer)
                return _rescheduleTimer._firedAction(timerFiredEvent);

            return _firedAction(timerFiredEvent);
        }

        WorkflowAction ITimer.StartFailed(TimerStartFailedEvent timerStartFailedEvent)
        {
            return _startFailureAction(timerStartFailedEvent);
        }

        WorkflowAction ITimer.CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            return _cancellationFailedAction(timerCancellationFailedEvent);
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisions()
        {
            if (!_canSchedule(this))
                return IsStartupItem()
                    ? Enumerable.Empty<WorkflowDecision>() 
                    : _falseAction(this).Decisions();

            return ScheduleDecisionsByIgnoringWhen();
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisionsByIgnoringWhen()
        {
            return new[] { new ScheduleTimerDecision(Identity, _fireAfterFunc(this), this == _rescheduleTimer) };
        }

        public override IEnumerable<WorkflowDecision> RescheduleDecisions(TimeSpan timeout)
        {
            _rescheduleTimer.FireAfter(timeout);
            return _rescheduleTimer.ScheduleDecisions();
        }

        public override IEnumerable<WorkflowDecision> CancelDecisions()
        {
            try
            {
                var cancelDecisions = Enumerable.Empty<WorkflowDecision>();
                if (!_invokedTimerCancelAction && IsActive)
                {
                    _invokedTimerCancelAction = true;
                    cancelDecisions = _timerCancelAction(this).Decisions();
                }

                return new []{new CancelTimerDecision(Identity)}.Concat(cancelDecisions);
            }
            finally
            {
                _invokedTimerCancelAction = false;
            }
        }

    }
}
