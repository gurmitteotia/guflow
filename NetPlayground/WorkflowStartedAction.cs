using System.Collections.Generic;
using System.Linq;

namespace NetPlayground
{
    public class WorkflowStartedAction : WorkflowAction
    {
        private const string _defaultCompleteResult = "Workflow completed as no schedulable item is found";
        private readonly WorkflowStartedEvent _workflowStartedEvent;
        private readonly HashSet<SchedulableItem> _allSchedulableItems;

        public WorkflowStartedAction(WorkflowStartedEvent workflowStartedEvent, HashSet<SchedulableItem> allSchedulableItems)
        {
            _workflowStartedEvent = workflowStartedEvent;
            _allSchedulableItems = allSchedulableItems;
        }

        protected override WorkflowDecision GetDecision()
        {
            var startupSchedulableItems = _allSchedulableItems.GetStartupItems();
            
            if (!startupSchedulableItems.Any())
                return new WorkflowCompleteDecision(_defaultCompleteResult);

            return new ScheduleItemsDecisions(startupSchedulableItems);
        }
    }
}