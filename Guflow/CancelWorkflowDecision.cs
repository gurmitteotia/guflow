using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal sealed class CancelWorkflowDecision : WorkflowDecision
    {
        private readonly string _details;

        public CancelWorkflowDecision(string details):base(true)
        {
            _details = details;
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
    }
}