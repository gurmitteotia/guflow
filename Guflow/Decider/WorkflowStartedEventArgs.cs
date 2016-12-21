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