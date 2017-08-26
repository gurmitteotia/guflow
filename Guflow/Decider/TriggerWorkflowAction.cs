using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Decider
{
    internal class TriggerWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _triggeringItem;
        private Func<WorkflowBranch, WorkflowItem> _findFirstJointItem = b => b.FindFirstJointItem();
        public TriggerWorkflowAction(WorkflowItem triggeringItem)
        {
            _triggeringItem = triggeringItem;
        }

        public void SetJumpedItem(WorkflowItem jumpedItem)
        {
            ValidateJump(jumpedItem);
            _findFirstJointItem = b => b.FindFirstJointItem(jumpedItem);
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var triggeredDecisions = new List<WorkflowDecision>();
            var childBranches = _triggeringItem.ChildBranches();
            foreach (var childBranch in childBranches)
            {
                var joinWorkflowItem = _findFirstJointItem(childBranch);
                if (joinWorkflowItem != null && joinWorkflowItem.AreAllParentBranchesInactive(_triggeringItem))
                    triggeredDecisions.Add(joinWorkflowItem.GetScheduleDecision());
            }
            return triggeredDecisions;
        }

        private void ValidateJump(WorkflowItem jumpedItem)
        {
            var triggeringItemBranches = _triggeringItem.ParentBranches().Concat(_triggeringItem.ChildBranches());

            if (!triggeringItemBranches.Any(b => b.Has(jumpedItem)))
                throw new OutOfBranchJumpException(string.Format(Resources.Invalid_jump, jumpedItem, _triggeringItem));
        }
    }
}