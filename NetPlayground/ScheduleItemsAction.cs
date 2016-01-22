using System.Collections.Generic;

namespace NetPlayground
{
    public class ScheduleItemsAction : WorkflowAction
    {
        private readonly IEnumerable<SchedulableItem> _schedulableItems;

        public ScheduleItemsAction(IEnumerable<SchedulableItem> schedulableItems)
        {
            _schedulableItems = schedulableItems;
        }

        protected override WorkflowDecision GetDecision()
        {
            return new ScheduleItemsDecisions(_schedulableItems);
        }
    }
}