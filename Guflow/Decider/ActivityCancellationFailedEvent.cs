// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represents activity cancellation failed event. It is raised when Amazon SWF has failed to process activity cancellation request.
    /// By default it cause the workflow to fail.
    /// </summary>
    public class ActivityCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly RequestCancelActivityTaskFailedEventAttributes _eventAttributes;
        internal ActivityCancellationFailedEvent(HistoryEvent activityCancellationFailedEvent) : base(activityCancellationFailedEvent.EventId)
        {
            _eventAttributes = activityCancellationFailedEvent.RequestCancelActivityTaskFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(_eventAttributes.ActivityId);
        }
        /// <summary>
        /// Returns cause, why activity cancellation request has failed.
        /// </summary>
        public string Cause => _eventAttributes.Cause.Value;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("ACTIVITY_CANCELLATION_FAILED", Cause);
        }
    }
}