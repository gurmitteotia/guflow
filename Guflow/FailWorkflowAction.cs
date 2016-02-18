using System.Collections.Generic;

namespace Guflow
{
    public class FailWorkflowAction : WorkflowAction
    {
        private readonly string _reason;
        private readonly string _detail;

        public FailWorkflowAction(string reason, string detail)
        {
            _reason = reason;
            _detail = detail;
        }

        public override bool Equals(object other)
        {
            var otherAction = other as FailWorkflowAction;
            if (otherAction == null)
                return false;

            return string.Equals(_reason, otherAction._reason) && string.Equals(_detail, otherAction._detail);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}{1}", _reason, _detail).GetHashCode();
        }

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {new FailWorkflowDecision(_reason, _detail)};
        }
    }
}