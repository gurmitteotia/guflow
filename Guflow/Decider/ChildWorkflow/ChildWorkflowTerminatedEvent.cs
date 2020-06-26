// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when child workflow is terminated.
    /// </summary>
    public sealed class ChildWorkflowTerminatedEvent : ChildWorkflowEvent
    {
        internal ChildWorkflowTerminatedEvent(HistoryEvent terminatedEvent, IEnumerable<HistoryEvent> allEvents)
            : base(terminatedEvent)
        {
            var attr = terminatedEvent.ChildWorkflowExecutionTerminatedEventAttributes;
            PopulateProperties(attr.WorkflowExecution.RunId, attr.InitiatedEventId, allEvents);
        }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("CHILD_WORKFLOW_TERMINATED",
                $"Name={WorkflowName}, Version={WorkflowVersion}, PositionalName={PositionalName}");
        }
    }
}