using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public class ContinueWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _completedWorkflowItem;
        private readonly IWorkflowContext _workflowContext;

        public ContinueWorkflowAction(WorkflowItem completedWorkflowItem, IWorkflowContext workflowContext)
        {
            _completedWorkflowItem = completedWorkflowItem;
            _workflowContext = workflowContext;
        }

        public override bool Equals(object other)
        {
            var otherAction = other as ContinueWorkflowAction;
            if (otherAction == null)
                return false;
            return _completedWorkflowItem.Equals(otherAction._completedWorkflowItem);
        }

        public override int GetHashCode()
        {
            return _completedWorkflowItem.GetHashCode();
        }

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var childItems = _completedWorkflowItem.GetChildlern();

            var filteredItems = childItems.Where(s => s.AllParentsAreProcessed(_workflowContext));

            return filteredItems.Select(f => f.GetDecision());
        }
    }
}