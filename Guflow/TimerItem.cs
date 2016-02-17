using System;

namespace Guflow
{
    public class TimerItem : WorkflowItem
    {
        private readonly IWorkflowItems _workflowItems;
        private TimeSpan _fireAfter= new TimeSpan();
        private Func<TimerFiredEvent, WorkflowAction> _onFiredAction;
 
        public TimerItem(string name, IWorkflowItems workflowItems):base(name,string.Empty,string.Empty)
        {
            _workflowItems = workflowItems;
            _onFiredAction = f=>new ContinueWorkflowAction(this,f,_workflowItems);
        }

        internal override WorkflowDecision GetDecision()
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsProcessed(IWorkflowContext workflowContext)
        {
            throw new System.NotImplementedException();
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
    }
}
