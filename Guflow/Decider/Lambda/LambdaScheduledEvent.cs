// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{

    /// <summary>
    /// Represents the lamdba scheduled event in SWF workflow history.
    /// </summary>
    public class LambdaScheduledEvent : LambdaEvent
    {
        internal LambdaScheduledEvent(HistoryEvent scheduledEvent) : base(scheduledEvent.EventId)
        {
            IsActive = true;
            PopulateProperties(scheduledEvent.EventId, new []{scheduledEvent});
        }
    }
}