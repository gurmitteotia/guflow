using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class CancelWorkflowDecision : WorkflowDecision
    {
        private readonly string _details;

        public CancelWorkflowDecision(string details)
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

        public override Decision Decision()
        {
            return new Decision
            {
                CancelWorkflowExecutionDecisionAttributes = new CancelWorkflowExecutionDecisionAttributes()
                {
                    Details = _details
                }
            };
        }
    }
}