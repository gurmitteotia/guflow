using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    internal class StartWorkflowAction : WorkflowAction
    {
        private const string _defaultCompleteResult = "Workflow completed as no schedulable item is found";
        private readonly IWorkflowItems _workflowItems;

        public StartWorkflowAction(IWorkflowItems workflowItems)
        {
            _workflowItems = workflowItems;
        }
        public override bool Equals(object other)
        {
            var otherAction = other as StartWorkflowAction;
            if (otherAction == null)
                return false;
            return _workflowItems.Equals(otherAction._workflowItems);
        }
        public override int GetHashCode()
        {
            return _workflowItems.GetHashCode();
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var startupWorkflowItems = _workflowItems.GetStartupWorkflowItems();

            if (!startupWorkflowItems.Any())
                return new []{new CompleteWorkflowDecision(_defaultCompleteResult)};

            return startupWorkflowItems.Select(s => s.GetDecision());
        }
    }
}