using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCancelRequestedEvent : WorkflowItemEvent
    {
        private readonly long _cancelRequestedEventId;

        internal ActivityCancelRequestedEvent(HistoryEvent activityCancelRequestedEvent) : base(activityCancelRequestedEvent.EventId)
        {
            _cancelRequestedEventId = activityCancelRequestedEvent.EventId;
            AwsIdentity = AwsIdentity.Raw(activityCancelRequestedEvent.ActivityTaskCancelRequestedEventAttributes.ActivityId);
            IsActive = true;
        }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            throw new NotSupportedException("Can not interpret activity cancel requested event.");
        }

        internal override bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            foreach (var itemEvent in workflowItemEvents.OfType<ActivityCancelledEvent>())
            {
                if (itemEvent.IsCancelledEventFor(_cancelRequestedEventId))
                    return true;
            }
            return false;
        }
    }
}