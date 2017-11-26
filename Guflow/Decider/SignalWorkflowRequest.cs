using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class SignalWorkflowRequest
    {
        public SignalWorkflowRequest(string workflowId, string signalName)
        {
            Ensure.NotNullAndEmpty(workflowId, "workflowId");
            Ensure.NotNullAndEmpty(signalName, "signalName");

            WorkflowId = workflowId;
            SignalName = signalName;
        }
        public string WorkflowId { get; private set; }
        public string SignalName { get; private set; }
        public string WorkflowRunId { get; set; }
        public object SignalInput { get; set; }

        internal SignalWorkflowExecutionRequest SwfFormat(string domainName)
        {
            return new SignalWorkflowExecutionRequest
            {
                Domain = domainName,
                RunId = WorkflowRunId,
                WorkflowId = WorkflowId,
                Input = SignalInput.ToAwsString(),
                SignalName = SignalName
            };
        }
    }
}