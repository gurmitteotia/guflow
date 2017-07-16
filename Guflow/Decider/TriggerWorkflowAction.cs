using System.Collections.Generic;

namespace Guflow.Decider
{
    internal class TriggerWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _triggeringItem;
        private readonly WorkflowItems _workflowItems;

        public TriggerWorkflowAction(WorkflowItem triggeringItem, WorkflowItems workflowItems)
        {
            _triggeringItem = triggeringItem;
            _workflowItems = workflowItems;
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var childBranches = _workflowItems.ChildBranchesOf(_triggeringItem);
            foreach (var childBranch in childBranches)
            {
                var workflowBranchJoint = childBranch.FindFirstJoint();

            }
        }
    }
}