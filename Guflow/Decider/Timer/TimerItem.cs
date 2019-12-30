// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    //TODO: Move out RescheduleTimer out of TimerItem. RescheduleTimer can be a decorator on Timer.
    internal sealed class TimerItem : WorkflowItem, IFluentTimerItem, ITimerItem
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

        private readonly ScheduleId _defaultScheduleId;
        private ScheduleId ResetScheduleId => Identity.ScheduleId(WorkflowHistoryEvents.WorkflowRunId + "Reset");

        private bool _invokedTimerCancelAction = false;// to avoid recursion.

        private TimerItem(Identity identity, ScheduleId defaultScheduleId, IWorkflow workflow)
                     : base(identity, workflow)
        {
            _defaultScheduleId = defaultScheduleId;
            _canSchedule = t => true;
            _falseAction = _ => IsStartupItem() ? WorkflowAction.Empty : new TriggerActions(this).FirstJoint();
            _timerCancelAction =_=>WorkflowAction.Empty;
            _fireAfterFunc = _ => _fireAfter;
        }

        public static TimerItem Reschedule(WorkflowItem ownerItem, ScheduleId scheduleId, IWorkflow workflow)
        {
            var identity = Identity.New(scheduleId.Name, scheduleId.Version, scheduleId.PositionalName);
            var timerItem = new TimerItem(identity, scheduleId, workflow);
            timerItem._rescheduleTimer = timerItem;
            timerItem.OnStartFailed(e => WorkflowAction.FailWorkflow("RESCHEDULE_TIMER_START_FAILED", e.Cause));
            timerItem.OnCancellationFailed(e => WorkflowAction.FailWorkflow("RESCHEDULE_TIMER_CANCELLATION_FAILED", e.Cause));
            timerItem.OnFired(e => WorkflowAction.Schedule(ownerItem));
            return timerItem;
        }
        public static TimerItem New(Identity identity, IWorkflow workflow)
        {
            var scheduleId = identity.ScheduleId();
            var timerItem = new TimerItem(identity, scheduleId, workflow);
            timerItem._rescheduleTimer = Reschedule(timerItem, scheduleId, workflow);
            timerItem.OnStartFailed(e => e.DefaultAction(workflow));
            timerItem.OnCancellationFailed(e => e.DefaultAction(workflow));
            timerItem.OnFired(e => e.DefaultAction(workflow));
            return timerItem;
        }
        public override WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false)
            => WorkflowHistoryEvents.LastTimerEvent(this, includeRescheduleTimerEvents);

        public override IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false) 
            => WorkflowHistoryEvents.AllTimerEvents(this, includeRescheduleTimerEvents);

        public override bool Has(ScheduleId id) => AllScheduleIds.Contains(id);
        
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

        public override WorkflowAction  Fired(TimerFiredEvent timerFiredEvent)
        {
            switch (timerFiredEvent.TimerType)
            {
                case TimerType.WorkflowItem:
                    return _firedAction(timerFiredEvent);
                case TimerType.Reschedule:
                    return RescheduleTimer._firedAction(timerFiredEvent);
                default:
                    return base.Fired(timerFiredEvent);
            }
        }

        public override WorkflowAction StartFailed(TimerStartFailedEvent timerStartFailedEvent)
        {
            return _startFailureAction(timerStartFailedEvent);
        }

        public override WorkflowAction CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            return _cancellationFailedAction(timerCancellationFailedEvent);
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisions()
        {
            if (!_canSchedule(this))
               return WorkflowDecisionsOnFalseWhen(_falseAction(this));

            return ScheduleDecisionsByIgnoringWhen();
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisionsByIgnoringWhen()
        {
            if(this== RescheduleTimer)
                return new[] { ScheduleTimerDecision.RescheduleTimer(ScheduleId , _fireAfterFunc(this)) };

            return new[] { ScheduleTimerDecision.WorkflowItem(ScheduleId , _fireAfterFunc(this)) };
        }

        public override IEnumerable<WorkflowDecision> RescheduleDecisions(TimeSpan timeout)
        {
            RescheduleTimer.FireAfter(timeout);
            return RescheduleTimer.ScheduleDecisions();
        }

        public override IEnumerable<WorkflowDecision> CancelDecisions()
        {
            try
            {
                var cancelDecisions = Enumerable.Empty<WorkflowDecision>();
                if (!_invokedTimerCancelAction && IsActive)
                {
                    _invokedTimerCancelAction = true;
                    cancelDecisions = _timerCancelAction(this).Decisions(Workflow);
                }

                return new []{new CancelTimerDecision(ScheduleId)}.Concat(cancelDecisions);
            }
            finally
            {
                _invokedTimerCancelAction = false;
            }
        }

        public WorkflowAction Reset() => ResetAction();
        public WorkflowAction Reschedule(TimeSpan timeout) => Reset(timeout);
        public WorkflowAction Reset(TimeSpan timeout) => ResetAction(timeout);

        private WorkflowAction ResetAction(TimeSpan? timeout= null)
        {
            if (!IsActive)
                throw new InvalidOperationException(
                    $"Can not reset the timer {this}. It should be already active for it be reset.");
            var lastTimerEvent = (TimerEvent) LastEvent(true);
            var rescheduleId = RescheduleId(lastTimerEvent.Id);
            return WorkflowAction.Custom(new CancelTimerDecision(lastTimerEvent.Id),
                 ScheduleTimerDecision.WorkflowItem(rescheduleId, timeout ?? lastTimerEvent.Timeout));
        }
        private ScheduleId RescheduleId(ScheduleId lastScheduleId) => AllScheduleIds.First(id=>id!=lastScheduleId);
        private ScheduleId[] AllScheduleIds => new[] {_defaultScheduleId, ResetScheduleId};

        protected override ScheduleId ScheduleId =>
            LastEvent(true) == null ? _defaultScheduleId : ((TimerEvent) LastEvent(true)).Id;

        protected override TimerItem RescheduleTimer => _rescheduleTimer;
    }
}
