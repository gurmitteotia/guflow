using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class FailWorkflowDecision : WorkflowDecision
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

        public override IEnumerable<Decision> Decisions()
        {
            throw new System.NotImplementedException();
        }
    }
}