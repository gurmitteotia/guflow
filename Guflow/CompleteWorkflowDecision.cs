using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal sealed class CompleteWorkflowDecision : WorkflowClosingDecision
    {
        private readonly string _result;
        private const int _mediumLow = 5;
        private const int _medium = 10;
        public CompleteWorkflowDecision(string result, bool proposal=false):base(proposal)
        {
            _result = result;
            Priority = proposal ? _mediumLow : _medium;
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

        internal override WorkflowAction ProvideFinalActionFrom(IWorkflowClosingActions workflowClosingActions)
        {
            return workflowClosingActions.OnCompletion(_result, Proposal);
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
