using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public class ContinueWorkflowAction : WorkflowAction
    {
        private readonly SchedulableItem _completedSchedulableItem;
        private readonly WorkflowEvent _completedWorkflowEvent;
        private readonly HashSet<SchedulableItem> _allSchedulableItems;

        public ContinueWorkflowAction(SchedulableItem completedSchedulableItem, WorkflowEvent completedWorkflowEvent, HashSet<SchedulableItem> allSchedulableItems)
        {
            _completedSchedulableItem = completedSchedulableItem;
            _completedWorkflowEvent = completedWorkflowEvent;
            _allSchedulableItems = allSchedulableItems;
        }
      

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var childItems = _allSchedulableItems.GetChildernOf(_completedSchedulableItem);

            var filteredItems = childItems.Where(s => s.AllParentsAreProcessed(_completedWorkflowEvent.WorkflowContext));

            return filteredItems.Select(f => f.GetDecision());
        }
    }
}