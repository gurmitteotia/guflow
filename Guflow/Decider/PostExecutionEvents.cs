// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    internal class PostExecutionEvents
    {
        private readonly Workflow _workflow;
        private readonly string _workflowId;
        private readonly string _workflowRunId;

        public PostExecutionEvents(Workflow workflow, string workflowId, string workflowRunId)
        {
            _workflow = workflow;
            _workflowId = workflowId;
            _workflowRunId = workflowRunId;
        }

        public void Completed(string result)
        {
            _workflow.OnCompleted(_workflowId, _workflowRunId, result);
        }

        public void Failed(string reason, string details)
        {
            _workflow.OnFailed(_workflowId, _workflowRunId, reason, details);
        }

        public void Cancelled(string details)
        {
            _workflow.OnCancelled(_workflowId, _workflowRunId, details);
        }

        public void Restarted()
        {
            _workflow.OnRestarted(_workflowId, _workflowRunId);
        }
    }
}