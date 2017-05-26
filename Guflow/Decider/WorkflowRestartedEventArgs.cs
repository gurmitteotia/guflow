using System;

namespace Guflow.Decider
{
    public sealed class WorkflowRestartedEventArgs : EventArgs
    {
        public WorkflowRestartedEventArgs(string workflowId, string workflowRunId)
        {
            WorkflowRunId = workflowRunId;
            WorkflowId = workflowId;
        }

        public string WorkflowRunId { get; private set; }
        public string WorkflowId { get; private set; }
    }
}