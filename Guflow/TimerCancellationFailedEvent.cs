using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly CancelTimerFailedEventAttributes _eventAttributes;
        private readonly Identity _identity;
        internal TimerCancellationFailedEvent(HistoryEvent timerCancellationFailedEvent)
        {
            _eventAttributes = timerCancellationFailedEvent.CancelTimerFailedEventAttributes;
            _identity = Identity.FromId(_eventAttributes.TimerId);
        }
        public string Cause { get { return _eventAttributes.Cause; } }
        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerCancellationFailed(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { throw new System.NotImplementedException(); }
        }
        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_identity);
        }
    }
}