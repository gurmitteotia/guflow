﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when SWF fails to cancel this workflow.
    /// </summary>
    public class WorkflowCancellationFailedEvent : WorkflowEvent
    {
        private readonly CancelWorkflowExecutionFailedEventAttributes _eventAttributes;
        internal WorkflowCancellationFailedEvent(HistoryEvent cancellationFailedEvent)
            : base(cancellationFailedEvent.EventId)
        {
            _eventAttributes = cancellationFailedEvent.CancelWorkflowExecutionFailedEventAttributes;
        }

        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_CANCEL_WORKFLOW", Cause);
        }
    }
}