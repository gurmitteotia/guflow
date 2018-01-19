// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    public class WorkflowStartedEventArgs : EventArgs
    {
        public WorkflowStartedEventArgs(WorkflowStartedEvent startEvent)
        {
            StartEvent = startEvent;
        }

        public WorkflowStartedEvent StartEvent { get; private set; }
    }
}