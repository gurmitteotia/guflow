﻿// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Reported when scheduled lambda function is successfully completed.
    /// </summary>
    public class LambdaCompletedEvent : LambdaEvent
    {
        internal LambdaCompletedEvent(HistoryEvent completedEvent, IEnumerable<HistoryEvent> eventGraph) 
            : base(completedEvent)
        {
            var attributes = completedEvent.LambdaFunctionCompletedEventAttributes;
            Result = attributes.Result;
            PopulateProperties(attributes.ScheduledEventId, eventGraph);
        }
        /// <summary>
        /// Returns the result of successfully completed lambda function.
        /// </summary>
        public string Result { get; }
        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.Continue(this);
        }
    }
}