using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly RequestCancelActivityTaskFailedEventAttributes _eventAttributes;
        internal ActivityCancellationFailedEvent(HistoryEvent activityCancellationFailedEvent)
        {
            _eventAttributes = activityCancellationFailedEvent.RequestCancelActivityTaskFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(_eventAttributes.ActivityId);
        }
        public string Cause { get { return _eventAttributes.Cause.Value; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCancellationFailed(this);
        }

        public override IWorkflowHistoryEvents WorkflowHistoryEvents
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}