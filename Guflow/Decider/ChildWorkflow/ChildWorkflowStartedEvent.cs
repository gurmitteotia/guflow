// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when child workflow is started.
    /// </summary>
    public sealed class ChildWorkflowStartedEvent : ChildWorkflowEvent
    {
        internal ChildWorkflowStartedEvent(HistoryEvent startedEvent, IEnumerable<HistoryEvent> allEvents) : base(startedEvent.EventId)
        {
            var attr = startedEvent.ChildWorkflowExecutionStartedEventAttributes;
            PopulateProperties(attr.WorkflowExecution.RunId, attr.InitiatedEventId, allEvents);
            IsActive = true;
        }

        //It is ignored for now. However based on user feedback we can allow user to take custom action.
        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return WorkflowAction.Empty;
        }
    }
}