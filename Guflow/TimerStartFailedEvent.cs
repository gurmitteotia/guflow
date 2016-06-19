using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerStartFailedEvent : WorkflowItemEvent
    {
        private readonly StartTimerFailedEventAttributes _startTimerFailedAttributes;
        public TimerStartFailedEvent(HistoryEvent startTimerFailedEvent)
        {
            _startTimerFailedAttributes = startTimerFailedEvent.StartTimerFailedEventAttributes;
            AwsIdentity =  AwsIdentity.Raw(_startTimerFailedAttributes.TimerId);
        }

        internal string Cause { get { return _startTimerFailedAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerStartFailed(this);
        }
    }
}