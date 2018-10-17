// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when the activity cancellation request is successfully recorded for an activity.
    /// </summary>
    public class ActivityCancelRequestedEvent : WorkflowItemEvent
    {

        internal ActivityCancelRequestedEvent(HistoryEvent activityCancelRequestedEvent) : base(activityCancelRequestedEvent.EventId)
        {
            AwsIdentity = AwsIdentity.Raw(activityCancelRequestedEvent.ActivityTaskCancelRequestedEventAttributes.ActivityId);
            IsActive = true;
        }
    }
}