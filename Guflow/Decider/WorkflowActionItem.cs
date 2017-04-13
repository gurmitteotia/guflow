using System.Collections.Generic;

namespace Guflow.Decider
{
    internal class WorkflowActionItem : WorkflowItem, IFluentWorkflowActionItem
    {
        private readonly WorkflowAction _workflowAction;

        public WorkflowActionItem(WorkflowAction workflowAction): base(Identity.New(""),)
        {
            _workflowAction = workflowAction;
        }

        public IFluentWorkflowActionItem After(string timerName)
        {
            throw new System.NotImplementedException();
        }

        public IFluentWorkflowActionItem After(string activityName, string activityVersion, string positionalName = "")
        {
            throw new System.NotImplementedException();
        }

        public override WorkflowItemEvent LastEvent
        {
            get { throw new System.NotImplementedException(); }
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents
        {
            get { throw new System.NotImplementedException(); }
        }

        internal override WorkflowDecision GetScheduleDecision()
        {
            throw new System.NotImplementedException();
        }

        internal override WorkflowDecision GetCancelDecision()
        {
            throw new System.NotImplementedException();
        }

        internal override WorkflowAction TimerFired(TimerFiredEvent timerFiredEvent)
        {
            throw new System.NotImplementedException();
        }

        internal override WorkflowAction TimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            throw new System.NotImplementedException();
        }

        internal override WorkflowAction TimerStartFailed(TimerStartFailedEvent timerStartFailedEvent)
        {
            throw new System.NotImplementedException();
        }

        internal override WorkflowAction TimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}