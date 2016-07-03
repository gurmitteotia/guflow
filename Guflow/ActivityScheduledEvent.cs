using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityScheduledEvent : ActivityEvent
    {
        internal ActivityScheduledEvent(HistoryEvent scheduledActivityEvent,IEnumerable<HistoryEvent> allHistoryEvents) : base(scheduledActivityEvent.EventId)
        {
            PopulateActivityFrom(allHistoryEvents, 0, scheduledActivityEvent.EventId);
            IsActive = true;
        }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            throw new NotSupportedException("Can not interpret activity scheduled event.");
        }
    }
}