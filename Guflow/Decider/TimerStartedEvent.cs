// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class TimerStartedEvent : TimerEvent
    {
        internal TimerStartedEvent(HistoryEvent timerStartedEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(timerStartedEvent.EventId)
        {
            PopulateProperties(timerStartedEvent.EventId,allHistoryEvents);
            IsActive = true;
        }
    }
}