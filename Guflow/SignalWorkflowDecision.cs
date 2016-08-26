using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal sealed class SignalWorkflowDecision : WorkflowDecision
    {
        private readonly string _signalName;
        private readonly string _input;
        private readonly string _workflowId;
        private readonly string _runId;

        public SignalWorkflowDecision(string signalName, string input, string workflowId, string runId) :base(false)
        {
            _signalName = signalName;
            _input = input;
            _workflowId = workflowId;
            _runId = runId;
        }

        internal override Decision Decision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.SignalExternalWorkflowExecution,
                SignalExternalWorkflowExecutionDecisionAttributes =
                    new SignalExternalWorkflowExecutionDecisionAttributes()
                    {
                        SignalName = _signalName,
                        Input = _input,
                        WorkflowId = _workflowId,
                        RunId = _runId
                    }
            };
        }
        private bool Equals(SignalWorkflowDecision other)
        {
            return string.Equals(_signalName, other._signalName) && string.Equals(_input, other._input) && string.Equals(_workflowId, other._workflowId) && string.Equals(_runId, other._runId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SignalWorkflowDecision && Equals((SignalWorkflowDecision)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _signalName.GetHashCode();
                hashCode = (hashCode * 397) ^ _input.GetHashCode();
                hashCode = (hashCode * 397) ^ _workflowId.GetHashCode();
                hashCode = (hashCode * 397) ^ (_runId==null?345:_runId.GetHashCode());
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Signal name {0}, input {1}, workflowId {2} and runid {3}", _signalName, _input,
                _workflowId, _runId);
        }
    }
}