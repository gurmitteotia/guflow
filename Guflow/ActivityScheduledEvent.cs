using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityScheduledEvent : ActivityEvent
    {
        internal ActivityScheduledEvent(HistoryEvent scheduledActivityEvent,IEnumerable<HistoryEvent> allHistoryEvents)
        {
            PopulateActivityFrom(allHistoryEvents, 0, scheduledActivityEvent.EventId);
            IsActive = true;
        }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            throw new NotSupportedException("Can not interpret activity scheduled event.");
        }
    }
}