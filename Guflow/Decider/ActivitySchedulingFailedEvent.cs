using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class ActivitySchedulingFailedEvent : WorkflowItemEvent
    {
        private readonly ScheduleActivityTaskFailedEventAttributes _eventAttributes;

        internal ActivitySchedulingFailedEvent(HistoryEvent schedulingFailedEvent) : base(schedulingFailedEvent.EventId)
        {
            _eventAttributes = schedulingFailedEvent.ScheduleActivityTaskFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(_eventAttributes.ActivityId);
        }
        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnActivitySchedulingFailed(this);
        }
    }
}