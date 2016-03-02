using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly RequestCancelActivityTaskFailedEventAttributes _eventAttributes;
        private readonly Identity _activityIdentity;
        internal ActivityCancellationFailedEvent(HistoryEvent activityCancellationFailedEvent)
        {
            _eventAttributes = activityCancellationFailedEvent.RequestCancelActivityTaskFailedEventAttributes;
            _activityIdentity = Identity.FromId(_eventAttributes.ActivityId);
        }
        public string Cause { get { return _eventAttributes.Cause.Value; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCancellationFailed(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { throw new System.NotImplementedException(); }
        }

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_activityIdentity);
        }
    }
}