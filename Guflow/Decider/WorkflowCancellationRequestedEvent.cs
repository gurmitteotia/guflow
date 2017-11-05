﻿using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
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