using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly CancelTimerFailedEventAttributes _eventAttributes;
        internal TimerCancellationFailedEvent(HistoryEvent timerCancellationFailedEvent)
        {
            _eventAttributes = timerCancellationFailedEvent.CancelTimerFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(_eventAttributes.TimerId);
        }
        public string Cause { get { return _eventAttributes.Cause; } }
        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerCancellationFailed(this);
        }

        public override IWorkflowHistoryEvents WorkflowHistoryEvents
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}