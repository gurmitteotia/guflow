using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent activity started event.
    /// </summary>
    public class ActivityStartedEvent : ActivityEvent
    {
        internal ActivityStartedEvent(HistoryEvent activityStartedEvent, IEnumerable<HistoryEvent> allHistoryEvents) : base(activityStartedEvent.EventId)
        {
            PopulateActivityFrom(allHistoryEvents,activityStartedEvent.EventId,activityStartedEvent.ActivityTaskStartedEventAttributes.ScheduledEventId);
            IsActive = true;
        }
    }
}