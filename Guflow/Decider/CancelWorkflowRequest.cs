// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class CancelWorkflowRequest
    {
        public CancelWorkflowRequest(string workflowId)
        {
            Ensure.NotNullAndEmpty(workflowId, "workflowId");
            WorkflowId = workflowId;
        }

        public string WorkflowId { get; private set; }
        public string WorkflowRunId { get; set; }

        internal RequestCancelWorkflowExecutionRequest SwfFormat(string domain)
        {
            return new RequestCancelWorkflowExecutionRequest()
            {
                Domain = domain,
                WorkflowId = WorkflowId,
                RunId = WorkflowRunId
            };
        }
    }
}