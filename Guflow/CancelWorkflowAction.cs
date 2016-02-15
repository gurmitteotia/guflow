using System.Collections.Generic;

namespace Guflow
{
    public class CancelWorkflowAction : WorkflowAction
    {
        private readonly string _details;

        public CancelWorkflowAction(string details)
        {
            _details = details;
        }

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {new CancelWorkflowDecision(_details),};
        }
    }
}