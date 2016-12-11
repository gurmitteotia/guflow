using System;

namespace Guflow
{
    public sealed class WorkflowCompletedEventArgs : EventArgs
    {
        internal WorkflowCompletedEventArgs( string workflowId, string workflowRunId, string result)
        {
            WorkflowRunId = workflowRunId;
            WorkflowId = workflowId;
            Result = result;
        }

        public string WorkflowRunId { get; private set; }
        public string WorkflowId { get; private set; }
        public string Result { get; private set; }
    }
}