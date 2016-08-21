using System.Collections.Generic;

namespace Guflow
{
    public class Signal
    {
        private readonly string _signalName;
        private readonly string _input;
        private readonly List<SignalWorkflowDecision> _decisions;

        internal Signal(string signalName, string input, List<SignalWorkflowDecision> decisions)
        {
            _signalName = signalName;
            _input = input;
            _decisions = decisions;
        }
       
        public void SendTo(string workflowId, string runId)
        {
            Ensure.NotNullAndEmpty(workflowId,"workflowId");
            _decisions.Add(new SignalWorkflowDecision(_signalName,_input,workflowId,runId)); 
        }

        public void ReplyTo(WorkflowSignaledEvent workflowSignaledEvent)
        {
            _decisions.Add(new SignalWorkflowDecision(_signalName, _input, workflowSignaledEvent.ExternalWorkflowId, workflowSignaledEvent.ExternalWorkflowRunid)); 
        }
    }
}