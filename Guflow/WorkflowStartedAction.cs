using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public class WorkflowStartedAction : WorkflowAction
    {
        private const string _defaultCompleteResult = "Workflow completed as no schedulable item is found";
        private readonly WorkflowStartedEvent _workflowStartedEvent;
        private readonly IWorkflowItems _workflowItems;

        public WorkflowStartedAction(WorkflowStartedEvent workflowStartedEvent, IWorkflowItems workflowItems)
        {
            _workflowStartedEvent = workflowStartedEvent;
            _workflowItems = workflowItems;
        }

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var startupSchedulableItems = _workflowItems.GetStartupItems();

            if (!startupSchedulableItems.Any())
                return new []{new CompleteWorkflowDecision(_defaultCompleteResult)};

            return startupSchedulableItems.Select(s => s.GetDecision());
        }
    }
}