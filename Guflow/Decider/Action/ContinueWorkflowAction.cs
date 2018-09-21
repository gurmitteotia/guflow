// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class ContinueWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _completedWorkflowItem;
        private const string DefaultWorkflowCompletedResult = "Workflow is completed.";
        public ContinueWorkflowAction(WorkflowItem completedWorkflowItem)
        {
            _completedWorkflowItem = completedWorkflowItem;
        }

        public override bool Equals(object other)
        {
            var otherAction = other as ContinueWorkflowAction;
            if (otherAction == null)
                return false;
            return _completedWorkflowItem.Equals(otherAction._completedWorkflowItem);
        }
        public override int GetHashCode()
        {
            return _completedWorkflowItem.GetHashCode();
        }
        internal override IEnumerable<WorkflowDecision> Decisions()
        {
            var childItems = _completedWorkflowItem.Children();
            if (!childItems.Any())
                return new[] { new CompleteWorkflowDecision(DefaultWorkflowCompletedResult, true) };

            var schedulableChildItems = childItems.Where(s => s.AreAllParentBranchesInactive(exceptBranchOf: _completedWorkflowItem));
            return schedulableChildItems.SelectMany(f => f.ScheduleDecisions());
        }

        internal override bool ReadyToScheduleChildren
        {
            get { return true; }
        }
    }
}