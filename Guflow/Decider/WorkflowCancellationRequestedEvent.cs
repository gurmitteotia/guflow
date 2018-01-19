// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represents a cancel request made to this workflow.
    /// </summary>
    public class WorkflowCancellationRequestedEvent : WorkflowEvent
    {
        private readonly WorkflowExecutionCancelRequestedEventAttributes _eventAttributes;

        internal WorkflowCancellationRequestedEvent(HistoryEvent cancellationRequestedEvent)
            : base(cancellationRequestedEvent.EventId)
        {
            _eventAttributes = cancellationRequestedEvent.WorkflowExecutionCancelRequestedEventAttributes;
        }

        public string Cause => _eventAttributes.Cause;

        public string ExternalWorkflowRunid => _eventAttributes.ExternalWorkflowExecution?.RunId;

        public string ExternalWorkflowId => _eventAttributes.ExternalWorkflowExecution?.WorkflowId;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnWorkflowCancellationRequested(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.CancelWorkflow(Cause);
        }
    }
}