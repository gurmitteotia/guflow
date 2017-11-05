﻿using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class ActivityCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly RequestCancelActivityTaskFailedEventAttributes _eventAttributes;
        internal ActivityCancellationFailedEvent(HistoryEvent activityCancellationFailedEvent) : base(activityCancellationFailedEvent.EventId)
        {
            _eventAttributes = activityCancellationFailedEvent.RequestCancelActivityTaskFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(_eventAttributes.ActivityId);
        }
        public string Cause => _eventAttributes.Cause.Value;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnActivityCancellationFailed(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("ACTIVITY_CANCELLATION_FAILED", Cause);
        }
    }
}