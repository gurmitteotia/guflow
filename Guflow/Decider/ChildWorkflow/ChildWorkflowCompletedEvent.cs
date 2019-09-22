// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represents the child workflow completed event 
    /// </summary>
    public sealed class ChildWorkflowCompletedEvent : ChildWorkflowEvent
    {
        private readonly ChildWorkflowExecutionCompletedEventAttributes _attr;
        internal ChildWorkflowCompletedEvent(HistoryEvent completedEvent, IEnumerable<HistoryEvent> allEvents) 
            : base(completedEvent)
        {
            _attr = completedEvent.ChildWorkflowExecutionCompletedEventAttributes;
            PopulateProperties(_attr.WorkflowExecution.RunId, _attr.InitiatedEventId, allEvents);
        }

        /// <summary>
        /// Returns completed result of child workflow.
        /// </summary>
        public string Result => _attr.Result;

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.Continue(this);
        }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }
    }
}