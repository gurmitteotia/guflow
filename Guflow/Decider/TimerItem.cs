using System;
using System.Collections.Generic;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal sealed class TimerItem : WorkflowItem, IFluentTimerItem, ITimerItem, ITimer
    {
        private TimeSpan _fireAfter= new TimeSpan();
        private Func<TimerFiredEvent, WorkflowAction> _onFiredAction;
        private Func<TimerCancellationFailedEvent, WorkflowAction> _onCanellationFailedAction;
        private Func<TimerStartFailedEvent, WorkflowAction> _onStartFailureAction;
        private Func<TimerCancelledEvent, WorkflowAction> _onTimerCancelledAction; 
        private Func<TimerItem, bool> _whenFunc;
        private TimerItem _rescheduleTimer;
        private TimerItem(Identity identity, IWorkflow workflow)
                     : base(identity, workflow)
        {
            _whenFunc = t => true;
        }

        public static TimerItem Reschedule(WorkflowItem ownerItem, Identity identity, IWorkflow workflow)
        {
            var timerItem = new TimerItem(identity, workflow);
            timerItem._rescheduleTimer = timerItem;
            timerItem.OnStartFailure(e => WorkflowAction.FailWorkflow("RESCHEDULE_TIMER_START_FAILED", e.Cause));
            timerItem.OnCancelled(e => WorkflowAction.CancelWorkflow("RESCHEDULE_TIMER_CANCELLED"));
            timerItem.OnFailedCancellation(e => WorkflowAction.FailWorkflow("RESCHEDULE_TIMER_CANCELLATION_FAILED", e.Cause));
            timerItem.OnFired(e => WorkflowAction.Schedule(ownerItem));
            return timerItem;
        }
        public static TimerItem New(Identity identity, IWorkflow workflow)
        {
            var timerItem = new TimerItem(identity, workflow);
            timerItem._rescheduleTimer = Reschedule(timerItem, identity, workflow);
            timerItem.OnStartFailure(e => e.DefaultAction(workflow));
            timerItem.OnCancelled(e => e.DefaultAction(workflow));
            timerItem.OnFailedCancellation(e => e.DefaultAction(workflow));
            timerItem.OnFired(e => e.DefaultAction(workflow));
            return timerItem;
        }

        public override WorkflowItemEvent LastEvent
        {
            get { return WorkflowHistoryEvents.LastTimerEventFor(this); }
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents
        {
            get { return WorkflowHistoryEvents.AllTimerEventsFor(this); }
        }
        public IFluentTimerItem FireAfter(TimeSpan fireAfter)
        {
            _fireAfter = fireAfter;
            return this;
        }
        public IFluentTimerItem When(Func<ITimerItem, bool> whenFunc)
        {
            Ensure.NotNull(whenFunc,"whenFunc");

            _whenFunc = whenFunc;
            return this;
        }
        public IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> onFiredAction)
        {
            Ensure.NotNull(onFiredAction, "onFiredAction");

            _onFiredAction = onFiredAction;
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
            var description = ActivityDescriptionAttribute.FindOn<TActivity>();
            return AfterActivity(description.Name, description.Version, positionalName);
        }
        public IFluentTimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> onCancelledFunc)
        {
            Ensure.NotNull(onCancelledFunc, "onCancelledFunc");

            _onTimerCancelledAction = onCancelledFunc;
            return this;
        }
        public IFluentTimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> onCancellationFailedFunc)
        {
            Ensure.NotNull(onCancellationFailedFunc, "onCancellationFailedFunc");

            _onCanellationFailedAction = onCancellationFailedFunc;
            return this;
        }

        public IFluentTimerItem OnStartFailure(Func<TimerStartFailedEvent, WorkflowAction> onStartFailureAction)
        {
            Ensure.NotNull(onStartFailureAction, "onStartFailureAction");
            _onStartFailureAction = onStartFailureAction;
            return this;
        }
        WorkflowAction ITimer.Fired(TimerFiredEvent timerFiredEvent)
        {
            if (timerFiredEvent.IsARescheduledTimer)
                return _rescheduleTimer._onFiredAction(timerFiredEvent);

            return _onFiredAction(timerFiredEvent);
        }

        WorkflowAction ITimer.Cancelled(TimerCancelledEvent timerCancelledEvent)
        {
            return _onTimerCancelledAction(timerCancelledEvent);
        }

        WorkflowAction ITimer.StartFailed(TimerStartFailedEvent timerStartFailedEvent)
        {
            return _onStartFailureAction(timerStartFailedEvent);
        }

        WorkflowAction ITimer.CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            return _onCanellationFailedAction(timerCancellationFailedEvent);
        }

        public override WorkflowDecision GetScheduleDecision()
        {
            if (!_whenFunc(this))
                return WorkflowDecision.Empty;

            return new ScheduleTimerDecision(Identity, _fireAfter, this == _rescheduleTimer);
        }

        public override WorkflowDecision GetRescheduleDecision(TimeSpan afterTimeout)
        {
            _rescheduleTimer.FireAfter(afterTimeout);
            return _rescheduleTimer.GetScheduleDecision();
        }

        public override WorkflowDecision GetCancelDecision()
        {
            return new CancelTimerDecision(Identity);
        }

    }
}
