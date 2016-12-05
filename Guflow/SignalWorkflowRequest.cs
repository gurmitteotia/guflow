using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class SignalWorkflowRequest
    {
        public SignalWorkflowRequest(string workflowId, string signalname)
        {
            Ensure.NotNullAndEmpty(workflowId, "workflowId");
            Ensure.NotNullAndEmpty(signalname, "signalName");

            WorkflowId = workflowId;
            Signalname = signalname;
        }
        public string WorkflowId { get; private set; }
        public string Signalname { get; private set; }
        public string WorkflowRunId { get; set; }
        public string SignalInput { get; set; }

        internal SignalWorkflowExecutionRequest SwfFormat(string domainName)
        {
            return new SignalWorkflowExecutionRequest
            {
                Domain = domainName,
                RunId = WorkflowRunId,
                WorkflowId = WorkflowId,
                Input = SignalInput,
                SignalName = Signalname
            };
        }
    }
}