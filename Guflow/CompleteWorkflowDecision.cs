using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal sealed class CompleteWorkflowDecision : WorkflowDecision
    {
        private readonly string _result;

        public CompleteWorkflowDecision(string result, bool proposal=false):base(true, proposal)
        {
            _result = result;
        }
        internal override Decision Decision()
        {
            return new Decision
            {
                DecisionType = DecisionType.CompleteWorkflowExecution,
                CompleteWorkflowExecutionDecisionAttributes = new CompleteWorkflowExecutionDecisionAttributes()
                {
                    Result = _result
                }
            };
        }
        public override string ToString()
        {
            return string.Format("{0} with result {1} and proposal {2}", GetType().Name, _result, Proposal);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CompleteWorkflowDecision && Equals((CompleteWorkflowDecision)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_result.GetHashCode() * 397) ^ Proposal.GetHashCode();
            }
        }
        private bool Equals(CompleteWorkflowDecision other)
        {
            return string.Equals(_result, other._result) && Proposal == other.Proposal;
        }

    }
}
