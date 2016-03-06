using System;

namespace Guflow
{
    public sealed class TimerItem : WorkflowItem
    {
        private TimeSpan _fireAfter= new TimeSpan();
        private Func<TimerFiredEvent, WorkflowAction> _onFiredAction;
        private Func<TimerCancellationFailedEvent, WorkflowAction> _onCanellationFailedAction; 
        
        internal TimerItem(string name, IWorkflowItems workflowItems):base(Identity.Timer(name),workflowItems)
        {
            _onFiredAction = f=>WorkflowAction.ContinueWorkflow(this,f.WorkflowContext);
            _onCanellationFailedAction = c => WorkflowAction.FailWorkflow("TIMER_CANCELLATION_FAILED", c.Cause);
        }
        internal override WorkflowDecision GetScheduleDecision()
        {
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

        protected override bool IsProcessed(IWorkflowContext workflowContext)
        {
            var timerEvent = workflowContext.LatestTimerEventFor(this);
            return timerEvent != null;
        }
        public TimerItem FireAfter(TimeSpan fireAfter)
        {
            _fireAfter = fireAfter;
            return this;
        }
        public TimerItem WhenFired(Func<TimerFiredEvent, WorkflowAction> onFiredAction)
        {
            _onFiredAction = onFiredAction;
            return this;
        }
        public TimerItem DependsOn(string timerName)
        {
            AddParent(Identity.Timer(timerName));
            return this;
        }

        public TimerItem DependsOn(string activityName, string activityVersion, string positionalName="")
        {
            AddParent(Identity.New(activityName, activityVersion, positionalName));
            return this;
        }

        public TimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> onCancelledAction)
        {
            OnTimerCancelledAction = onCancelledAction;
            return this;
        }
        internal WorkflowAction CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            return _onCanellationFailedAction(timerCancellationFailedEvent);
        }

        public TimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> onCancellationFailedAction)
        {
            _onCanellationFailedAction = onCancellationFailedAction;
            return this;
        }
    }
}
