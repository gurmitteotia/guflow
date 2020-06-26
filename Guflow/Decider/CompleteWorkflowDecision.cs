// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class CompleteWorkflowDecision : WorkflowClosingDecision
    {
        private readonly string _result;
        private const int VeryLow = 5;
        private const int Low = 10;
        public CompleteWorkflowDecision(string result, bool proposal=false):base(proposal)
        {
            _result = result;
            Priority = proposal ? VeryLow : Low;
        }
        internal override Decision SwfDecision()
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

        internal override void Raise(PostExecutionEvents postExecutionEvents)
        {
            postExecutionEvents.Completed(_result);
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
            return string.IsNullOrEmpty(_result) ? GetType().GetHashCode() : _result.GetHashCode() ^ Proposal.GetHashCode();
        }

        private bool Equals(CompleteWorkflowDecision other)
        {
            return string.Equals(_result, other._result) && Proposal == other.Proposal;
        }

    }
}
