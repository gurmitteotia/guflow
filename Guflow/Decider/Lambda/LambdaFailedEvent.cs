// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// This event is raised when lambda function is failed. 
    /// </summary>
    public class LambdaFailedEvent : LambdaEvent
    {
        internal LambdaFailedEvent(HistoryEvent failedEvent, IEnumerable<HistoryEvent> allEvents) 
            : base(failedEvent)
        {
            var attr = failedEvent.LambdaFunctionFailedEventAttributes;
            Reason = attr.Reason;
            Details = attr.Details;
            PopulateProperties(attr.ScheduledEventId, allEvents);
        }
        /// <summary>
        /// Gets the reason on failure of lamdba function.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets the details on failure of lambda function.
        /// </summary>
        public string Details { get; }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return WorkflowAction.FailWorkflow(Reason, Details);
        }
    }
}