using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ScheduleItemsDecisions : WorkflowDecision
    {
        private readonly IEnumerable<SchedulableItem> _schedulableItems;

        public ScheduleItemsDecisions(IEnumerable<SchedulableItem> schedulableItems)
        {
            _schedulableItems = schedulableItems;
        }

        public override IEnumerable<Decision> Decisions()
        {
            return _schedulableItems.Select(i => i.GetDecision());
        }
    }
}