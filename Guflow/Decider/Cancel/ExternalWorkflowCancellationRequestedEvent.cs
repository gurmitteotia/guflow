﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when cancel request is delivered to external workflow.
    /// </summary>
    public class ExternalWorkflowCancellationRequestedEvent : WorkflowItemEvent
    {
        internal ExternalWorkflowCancellationRequestedEvent(HistoryEvent cancelRequested) 
            : base(cancelRequested.EventId)
        {
            var attr = cancelRequested.ExternalWorkflowExecutionCancelRequestedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(attr.WorkflowExecution.WorkflowId);
            IsActive = true;
        }
    }
}