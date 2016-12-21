using System;

namespace Guflow.Decider
{
    public sealed class WorkflowCompletedEventArgs : EventArgs
    {
        public WorkflowCompletedEventArgs( string workflowId, string workflowRunId, string result)
        {
            WorkflowId = workflowId;
            WorkflowRunId = workflowRunId;
            Result = result;
        }

        public string WorkflowRunId { get; private set; }
        public string WorkflowId { get; private set; }
        public string Result { get; private set; }
    }
}