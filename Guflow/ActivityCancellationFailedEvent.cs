using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly RequestCancelActivityTaskFailedEventAttributes _eventAttributes;
        internal ActivityCancellationFailedEvent(HistoryEvent activityCancellationFailedEvent) : base(activityCancellationFailedEvent.EventId)
        {
            _eventAttributes = activityCancellationFailedEvent.RequestCancelActivityTaskFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(_eventAttributes.ActivityId);
        }
        public string Cause { get { return _eventAttributes.Cause.Value; } }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCancellationFailed(this);
        }
    }
}