using System.Collections.Generic;
using System.Linq;

namespace NetPlayground
{
    public class ScheduleItemsAction : WorkflowAction
    {
        private readonly IEnumerable<SchedulableItem> _schedulableItems;
        private readonly IWorkflowContext _workflowContext;

        public ScheduleItemsAction(IEnumerable<SchedulableItem> schedulableItems, IWorkflowContext workflowContext)
        {
            _schedulableItems = schedulableItems;
            _workflowContext = workflowContext;
        }

        protected override WorkflowDecision GetDecision()
        {

            var filteredItems = _schedulableItems.Where(s => s.AllParentsAreProcessed(_workflowContext));

            return new ScheduleItemsDecisions(filteredItems);
        }
    }
}