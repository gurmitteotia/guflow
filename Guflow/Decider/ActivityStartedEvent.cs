using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class ActivityStartedEvent : ActivityEvent
    {
        internal ActivityStartedEvent(HistoryEvent activityStartedEvent, IEnumerable<HistoryEvent> allHistoryEvents) : base(activityStartedEvent.EventId)
        {
            PopulateActivityFrom(allHistoryEvents,activityStartedEvent.EventId,activityStartedEvent.ActivityTaskStartedEventAttributes.ScheduledEventId);
            IsActive = true;
        }
        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            throw new NotSupportedException("Can not interpret activity started event.");
        }
    }
}