using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public class ContinueWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _completedWorkflowItem;
        private readonly WorkflowEvent _completedWorkflowEvent;
        private readonly IWorkflowItems _workflowItems;

        public ContinueWorkflowAction(WorkflowItem completedWorkflowItem, WorkflowEvent completedWorkflowEvent, IWorkflowItems workflowItems)
        {
            _completedWorkflowItem = completedWorkflowItem;
            _completedWorkflowEvent = completedWorkflowEvent;
            _workflowItems = workflowItems;
        }
      

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var childItems = _workflowItems.GetChildernOf(_completedWorkflowItem);

            var filteredItems = childItems.Where(s => s.AllParentsAreProcessed(_completedWorkflowEvent.WorkflowContext));

            return filteredItems.Select(f => f.GetDecision());
        }
    }
}