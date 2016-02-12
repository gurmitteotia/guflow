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

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var startupSchedulableItems = _allSchedulableItems.GetStartupItems();

            if (!startupSchedulableItems.Any())
                return new []{new CompleteWorkflowDecision(_defaultCompleteResult)};

            return startupSchedulableItems.Select(s => s.GetDecision());
        }
    }
}