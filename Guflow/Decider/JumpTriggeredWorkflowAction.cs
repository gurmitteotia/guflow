using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Decider
{
    internal class JumpTriggeredWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _triggeringItem;
        private readonly WorkflowItem _jumpedItem;

        public JumpTriggeredWorkflowAction(WorkflowItem triggeringItem, WorkflowItem jumpedItem)
        {
            _triggeringItem = triggeringItem;
            _jumpedItem = jumpedItem;
            ValidateJump();
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var triggeredDecisions = new List<WorkflowDecision>();
            var childBranches = _triggeringItem.ChildBranches();
            foreach (var childBranch in childBranches)
            {
                var joinWorkflowItem = childBranch.FindFirstJointItemUpTo(_jumpedItem);
                if(joinWorkflowItem != null && joinWorkflowItem.AreAllParentBranchesInactive(_triggeringItem))
                    triggeredDecisions.Add(joinWorkflowItem.GetScheduleDecision());
            }
            return triggeredDecisions;
        }

        private void ValidateJump()
        {
            var triggeringItemBranches = _triggeringItem.ParentBranches().Concat(_triggeringItem.ChildBranches());

            if(!triggeringItemBranches.Any(b=>b.Has(_jumpedItem)))
                throw new OutOfBranchJumpException(string.Format(Resources.Invalid_jump, _jumpedItem, _triggeringItem));
        }
    }
}