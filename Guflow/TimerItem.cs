using System;

namespace Guflow
{
    internal sealed class TimerItem : WorkflowItem, IFluentTimerItem, ITimerItem
    {
        private TimeSpan _fireAfter= new TimeSpan();
        private Func<TimerFiredEvent, WorkflowAction> _onFiredAction;
        private Func<TimerCancellationFailedEvent, WorkflowAction> _onCanellationFailedAction;
        private Func<TimerStartFailedEvent, WorkflowAction> _onStartFailureAction; 
        private Func<TimerItem, bool> _whenFunc;
        internal TimerItem(string name, IWorkflowItems workflowItems):base(Identity.Timer(name),workflowItems)
        {
            _onFiredAction = f=>WorkflowAction.ContinueWorkflow(this);
            _onCanellationFailedAction = c => WorkflowAction.FailWorkflow("TIMER_CANCELLATION_FAILED", c.Cause);
            _onStartFailureAction = c => WorkflowAction.FailWorkflow("START_TIMER_FAILED", c.Cause);
            _whenFunc = t => true;
        }
        public TimerEvent LatestEvent
        {
            get { return WorkflowHistoryEvents.LatestEventFor(this); }
        }
        public IFluentTimerItem FireAfter(TimeSpan fireAfter)
        {
            _fireAfter = fireAfter;
            return this;
        }
        public IFluentTimerItem When(Func<ITimerItem, bool> whenFunc)
        {
            _whenFunc = whenFunc;
            return this;
        }
        public IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> onFiredAction)
        {
            _onFiredAction = onFiredAction;
            return this;
        }
        public IFluentTimerItem DependsOn(string timerName)
        {
            AddParent(Identity.Timer(timerName));
            return this;
        }

        public IFluentTimerItem DependsOn(string activityName, string activityVersion, string positionalName = "")
        {
            AddParent(Identity.New(activityName, activityVersion, positionalName));
            return this;
        }

        public IFluentTimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> onCancelledAction)
        {
            OnTimerCancelledAction = onCancelledAction;
            return this;
        }
        public IFluentTimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> onCancellationFailedAction)
        {
            _onCanellationFailedAction = onCancellationFailedAction;
            return this;
        }

        public IFluentTimerItem OnStartFailure(Func<TimerStartFailedEvent, WorkflowAction> onStartFailureAction)
        {
            _onStartFailureAction = onStartFailureAction;
            return this;
        }

        internal override WorkflowDecision GetScheduleDecision()
        {
            if(!_whenFunc(this))
                return WorkflowDecision.Empty;

            return new ScheduleTimerDecision(Identity, _fireAfter);
        }
        internal override WorkflowDecision GetCancelDecision()
        {
            return new CancelTimerDecision(Identity);
        }

        internal override WorkflowAction TimerFired(TimerFiredEvent timerFiredEvent)
        {
            if (timerFiredEvent.IsARescheduledTimer)
                return WorkflowAction.Schedule(this);

            return _onFiredAction(timerFiredEvent);
        }
        protected override bool IsProcessed()
        {
            var timerEvent = LatestEvent;
            return timerEvent != null;
        }
        internal WorkflowAction CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            return _onCanellationFailedAction(timerCancellationFailedEvent);
        }
    }
}
