﻿using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class FailWorkflowDecision : WorkflowClosingDecision
    {
        private readonly string _reason;
        private readonly string _details;
        private const int _high = 20;
        public FailWorkflowDecision(string reason, string details)
        {
            _reason = reason;
            _details = details;
            Priority = _high;
        }

        public override bool Equals(object other)
        {
            var otherDecision = other as FailWorkflowDecision;
            if (otherDecision == null)
                return false;
            return string.Equals(_reason, otherDecision._reason) &&
                   string.Equals(_details, otherDecision._details);
        }
        public override int GetHashCode()
        {
            return string.Format("{0}{1}", _reason, _details).GetHashCode();
        }
        internal override Decision Decision()
        {
            return new Decision
            {
                DecisionType = DecisionType.FailWorkflowExecution,
                FailWorkflowExecutionDecisionAttributes = new FailWorkflowExecutionDecisionAttributes()
                {
                    Reason = _reason,
                    Details = _details
                }
            };
        }
        public override string ToString()
        {
            return string.Format("{0} with reason {1} and details {2}", GetType().Name, _reason, _details);
        }

        internal override WorkflowAction ProvideFinalActionFrom(IWorkflowClosingActions workflowClosingActions)
        {
            return workflowClosingActions.OnFailure(_reason, _details);
        }

        internal override void Raise(PostExecutionEvents postExecutionEvents)
        {
            postExecutionEvents.Failed(_reason, _details);
        }
    }
}