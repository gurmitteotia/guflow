// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent timer cancelled event.
    /// </summary>
    public class TimerCancelledEvent : TimerEvent
    {
        internal TimerCancelledEvent(HistoryEvent timerCancelledEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(timerCancelledEvent.EventId)
        {
            var eventAttributes = timerCancelledEvent.TimerCanceledEventAttributes;
            PopulateProperties(eventAttributes.StartedEventId, allHistoryEvents);
        }
    }
}