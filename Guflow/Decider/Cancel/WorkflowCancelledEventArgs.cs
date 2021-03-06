﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    public class WorkflowCancelledEventArgs : EventArgs
    {
        public WorkflowCancelledEventArgs(string workflowId, string workflowRunId, string details)
        {
            WorkflowId = workflowId;
            WorkflowRunId = workflowRunId;
            Details = details;
        }

        public string WorkflowRunId { get; private set; }
        public string WorkflowId { get; private set; }
        public string Details { get; private set; }
    }
}