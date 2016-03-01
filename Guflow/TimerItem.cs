using System;

namespace Guflow
{
    public sealed class TimerItem : WorkflowItem
    {
        private TimeSpan _fireAfter= new TimeSpan();
        private Func<TimerFiredEvent, WorkflowAction> _onFiredAction;

        internal TimerItem(string name, IWorkflowItems workflowItems):base(Identity.Timer(name),workflowItems)
        {
            _onFiredAction = f=>new ContinueWorkflowAction(this,f.WorkflowContext);
        }

        internal override WorkflowDecision GetScheduleDecision()
        {
            return new ScheduleTimerDecision(Identity, _fireAfter);
        }

        internal override WorkflowDecision GetCancelDecision()
        {
            return new CancelTimerDecision(Identity);
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

        internal WorkflowAction Fired(TimerFiredEvent timerFiredEvent)
        {
            return _onFiredAction(timerFiredEvent);
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
    }
}
