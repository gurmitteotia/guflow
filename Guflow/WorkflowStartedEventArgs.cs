using System;

namespace Guflow
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