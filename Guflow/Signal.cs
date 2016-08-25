using System.Collections.Generic;

namespace Guflow
{
    public class Signal
    {
        private readonly string _signalName;
        private readonly string _input;

        internal Signal(string signalName, object input)
        {
            _signalName = signalName;
            _input = input.ToAwsString();
        }
       
        public WorkflowAction SendTo(string workflowId, string runId)
        {
            Ensure.NotNullAndEmpty(workflowId,"workflowId");
            return WorkflowAction.Signal(_signalName, _input, workflowId, runId);
        }

        public WorkflowAction ReplyTo(WorkflowSignaledEvent workflowSignaledEvent)
        {
            Ensure.NotNull(workflowSignaledEvent, "workflowSignaledEvent");
            return WorkflowAction.Signal(_signalName, _input, workflowSignaledEvent.ExternalWorkflowId, workflowSignaledEvent.ExternalWorkflowRunid);
        }
    }
}