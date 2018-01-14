using Guflow.Properties;

namespace Guflow.Decider
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
       
        public WorkflowAction ForWorkflow(string workflowId, string runId)
        {
            Ensure.NotNullAndEmpty(workflowId,"workflowId");
            return WorkflowAction.Signal(_signalName, _input, workflowId, runId);
        }

        public WorkflowAction ReplyTo(WorkflowSignaledEvent workflowSignaledEvent)
        {
            Ensure.NotNull(workflowSignaledEvent, "workflowSignaledEvent");
            if(!workflowSignaledEvent.IsSentByWorkflow)
                throw new SignalException(Resources.Can_not_reply_to_signal);

            return WorkflowAction.Signal(_signalName, _input, workflowSignaledEvent.ExternalWorkflowId, workflowSignaledEvent.ExternalWorkflowRunid);
        }
    }
}