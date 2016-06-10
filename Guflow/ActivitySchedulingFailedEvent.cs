using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivitySchedulingFailedEvent : WorkflowItemEvent
    {
        private readonly ScheduleActivityTaskFailedEventAttributes _eventAttributes;

        internal ActivitySchedulingFailedEvent(HistoryEvent schedulingFailedEvent)
        {
            _eventAttributes = schedulingFailedEvent.ScheduleActivityTaskFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(_eventAttributes.ActivityId);
        }
        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivitySchedulingFailed(this);
        }
    }
}