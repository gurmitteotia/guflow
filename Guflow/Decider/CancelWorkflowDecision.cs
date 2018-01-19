// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class CancelWorkflowDecision : WorkflowClosingDecision
    {
        private readonly string _details;
        private const int _mediumHigh = 15;
        public CancelWorkflowDecision(string details)
        {
            _details = details;
            Priority = _mediumHigh;
        }

        public override bool Equals(object other)
        {
            var otherDecision = other as CancelWorkflowDecision;
            if (otherDecision == null)
                return false;
            return string.Equals(_details, otherDecision._details);
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(_details) ? GetType().GetHashCode() : _details.GetHashCode();
        }

        internal override Decision Decision()
        {
            return new Decision
            {
                DecisionType = DecisionType.CancelWorkflowExecution,
                CancelWorkflowExecutionDecisionAttributes = new CancelWorkflowExecutionDecisionAttributes()
                {
                    Details = _details
                }
            };
        }

        public override string ToString()
        {
            return string.Format("{0} with details {1}", GetType().Name, _details);
        }

        internal override WorkflowAction ProvideFinalActionFrom(IWorkflowClosingActions workflowClosingActions)
        {
            return workflowClosingActions.OnCancellation(_details);
        }

        internal override void Raise(PostExecutionEvents postExecutionEvents)
        {
            postExecutionEvents.Cancelled(_details);
        }
    }
}