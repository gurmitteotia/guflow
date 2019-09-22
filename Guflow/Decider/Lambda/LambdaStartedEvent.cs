// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Recorded in SWF workflow history when scheduled lamdba function is started.
    /// </summary>
    public class LambdaStartedEvent : LambdaEvent
    {
        internal LambdaStartedEvent(HistoryEvent startedEvent, IEnumerable<HistoryEvent> allEvents) 
            : base(startedEvent)
        {
            IsActive = true;
            PopulateProperties(startedEvent.LambdaFunctionStartedEventAttributes.ScheduledEventId, allEvents);
        }
    }
}