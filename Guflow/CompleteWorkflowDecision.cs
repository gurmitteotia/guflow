using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal sealed class CompleteWorkflowDecision : WorkflowDecision
    {
        private readonly string _result;
        private readonly bool _proposal;

        public CompleteWorkflowDecision(string result, bool proposal=false)
        {
            _result = result;
            _proposal = proposal;
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
            return string.Format("{0} with result {1} and proposal {2}", GetType().Name, _result,_proposal);
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
                return (_result.GetHashCode() * 397) ^ _proposal.GetHashCode();
            }
        }

        internal override bool IsCompaitbleWith(IEnumerable<WorkflowDecision> workflowDecisions)
        {
            if (_proposal)
                return !workflowDecisions.Any();

            return !workflowDecisions.Any();
        }

        private bool Equals(CompleteWorkflowDecision other)
        {
            return string.Equals(_result, other._result) && _proposal == other._proposal;
        }

    }
}
