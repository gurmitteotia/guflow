using System;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCancelRequestedEvent : WorkflowItemEvent
    {
        public ActivityCancelRequestedEvent(HistoryEvent activityCancelRequestedEvent) : base(activityCancelRequestedEvent.EventId)
        {
            AwsIdentity = AwsIdentity.Raw(activityCancelRequestedEvent.ActivityTaskCancelRequestedEventAttributes.ActivityId);
            IsActive = true;
        }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            throw new NotSupportedException("Can not interpret activity cancel requested event.");
        }
    }
}