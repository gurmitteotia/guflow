// /Copyright (c) Gurmit Teotia. Please .see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when the lamdba function is failed to complete in a given timeout.
    /// </summary>
    public class LambdaTimedoutEvent : LambdaEvent
    {
        internal LambdaTimedoutEvent(HistoryEvent timedoutEvent, IEnumerable<HistoryEvent> allEvents) 
            : base(timedoutEvent)
        {
            var attr = timedoutEvent.LambdaFunctionTimedOutEventAttributes;
            TimedoutType = attr.TimeoutType;
            PopulateProperties(attr.ScheduledEventId, allEvents);
        }

        /// <summary>
        /// Returns the timeout type for lamdba function.
        /// </summary>
        public string TimedoutType { get; }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return WorkflowAction.FailWorkflow("LAMBDA_FUNCTION_TIMED_OUT", TimedoutType);
        }
    }
}