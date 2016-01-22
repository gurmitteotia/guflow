using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class WorkflowStartedAction : WorkflowAction
    {
        private readonly IEnumerable<SchedulableItem> _schedulableItems;

        public WorkflowStartedAction(IEnumerable<SchedulableItem> schedulableItems)
        {
            _schedulableItems = schedulableItems;
        }

        protected override WorkflowDecision GetDecision()
        {
            if (!_schedulableItems.Any())
                return new WorkflowCompleteDecision();

            return new ScheduleItemsDecisions(_schedulableItems);
        }
    }
}