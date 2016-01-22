using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ScheduleItemWorkflowDecision : WorkflowDecision
    {
        private readonly IEnumerable<SchedulableItem> _schedulableItems;

        public ScheduleItemWorkflowDecision(IEnumerable<SchedulableItem> schedulableItems)
        {
            _schedulableItems = schedulableItems;
        }

        public override IEnumerable<Decision> Decisions()
        {
            return _schedulableItems.Select(s => s.GetDecision());
        }
    }
}