// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent activity started event.
    /// </summary>
    public class ActivityStartedEvent : ActivityEvent
    {
        internal ActivityStartedEvent(HistoryEvent activityStartedEvent, IEnumerable<HistoryEvent> allHistoryEvents) 
            : base(activityStartedEvent)
        {
            PopulateAttributes(allHistoryEvents,activityStartedEvent.EventId,activityStartedEvent.ActivityTaskStartedEventAttributes.ScheduledEventId);
            IsActive = true;
        }
    }
}