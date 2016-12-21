using System;

namespace Guflow.Decider
{
    public class WorkflowFailedEventArgs : EventArgs
    {
        public WorkflowFailedEventArgs(string workflowId, string workflowRunId, string reason, string details)
        {
            WorkflowRunId = workflowRunId;
            WorkflowId = workflowId;
            Reason = reason;
            Details = details;
        }

        public string WorkflowRunId { get; private set; }
        public string WorkflowId { get; private set; }
        public string Reason { get; private set; }
        public string Details { get; private set; }
    }
}