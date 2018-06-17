// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when SWF is failed to start the scheduled lamdba function.
    /// </summary>
    public class LambdaStartFailedEvent : LambdaEvent
    {
        internal LambdaStartFailedEvent(HistoryEvent failedEvent, IEnumerable<HistoryEvent> allEvents) : base(failedEvent.EventId)
        {
            var attr = failedEvent.StartLambdaFunctionFailedEventAttributes;
            Cause = attr.Cause;
            Message = attr.Message;
            PopulateProperties(attr.ScheduledEventId, allEvents);
        }
        /// <summary>
        /// Returns the cause for why lamdba function was failed to start.
        /// </summary>
        public string Cause { get; }

        /// <summary>
        /// Return the details message about failure.
        /// </summary>
        public string Message { get; }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }
    }
}