using System.Collections.Generic;
using System.Linq;

namespace NetPlayground
{
    public class WorkflowStartedAction : WorkflowAction
    {
        private const string _defaultCompleteResult = "Workflow completed as no schedulable item is found";
        private readonly IEnumerable<SchedulableItem> _schedulableItems;

        public WorkflowStartedAction(IEnumerable<SchedulableItem> schedulableItems)
        {
            _schedulableItems = schedulableItems;
        }

        protected override WorkflowDecision GetDecision()
        {
            if (!_schedulableItems.Any())
                return new WorkflowCompleteDecision(_defaultCompleteResult);

            return new ScheduleItemsDecisions(_schedulableItems);
        }
    }
}