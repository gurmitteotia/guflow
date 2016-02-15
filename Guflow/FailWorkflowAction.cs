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

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {new FailWorkflowDecision(_reason, _detail)};
        }
    }
}