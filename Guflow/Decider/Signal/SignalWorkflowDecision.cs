// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
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

        internal override Decision SwfDecision()
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
            return string.Equals(_signalName, other._signalName) && string.Equals(_input, other._input) 
                                                                 && string.Equals(_workflowId, other._workflowId) && string.Equals(_runId, other._runId);
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
            return $"Signal name {_signalName}, input {_input}, workflowId {_workflowId} and runid {_runId}";
        }
    }
}