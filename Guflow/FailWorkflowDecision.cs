using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class FailWorkflowDecision : WorkflowDecision
    {
        private readonly string _reason;
        private readonly string _detail;

        public FailWorkflowDecision(string reason, string detail)
        {
            _reason = reason;
            _detail = detail;
        }

        public override bool Equals(object other)
        {
            var otherDecision = other as FailWorkflowDecision;
            if (otherDecision == null)
                return false;
            return string.Equals(_reason, otherDecision._reason) &&
                   string.Equals(_detail, otherDecision._detail);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}{1}", _reason, _detail).GetHashCode();
        }

        public override Decision Decision()
        {
            return new Decision
            {
                DecisionType = DecisionType.FailWorkflowExecution,
                FailWorkflowExecutionDecisionAttributes = new FailWorkflowExecutionDecisionAttributes()
                {
                    Reason = _reason,
                    Details = _detail
                }
            };
        }
    }
}