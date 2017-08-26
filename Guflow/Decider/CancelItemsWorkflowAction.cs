using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class CancelItemsWorkflowAction : WorkflowAction
    {
        private readonly IEnumerable<WorkflowItem> _workflowItems;

        public CancelItemsWorkflowAction(IEnumerable<WorkflowItem> workflowItems)
        {
            _workflowItems = workflowItems;
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _workflowItems.Select(w => w.GetCancelDecision());
        }
    }
}