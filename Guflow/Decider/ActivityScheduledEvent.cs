// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class ActivityScheduledEvent : ActivityEvent
    {
        internal ActivityScheduledEvent(HistoryEvent scheduledActivityEvent,IEnumerable<HistoryEvent> allHistoryEvents) : base(scheduledActivityEvent.EventId)
        {
            PopulateActivityFrom(allHistoryEvents, 0, scheduledActivityEvent.EventId);
            IsActive = true;
        }
    }
}