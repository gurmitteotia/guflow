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
        internal override IEnumerable<WorkflowDecision> Decisions(IWorkflow workflow)
        {
            var decisions = new List<WorkflowDecision>();
            var childItems = _completedWorkflowItem.Children().ToArray();
            if (!childItems.Any())
                return new[] { new CompleteWorkflowDecision(DefaultWorkflowCompletedResult, true) };

            var schedulableChildItems = childItems.Where(s => s.AreAllParentBranchesInactive(exceptBranchOf: _completedWorkflowItem));
            //Current continue item is tracked to avoid recursion. Need to better represent the intention.
            foreach (var childItem in schedulableChildItems)
            {
                if (_completedWorkflowItem.HasContinueItem(childItem)) continue;
                _completedWorkflowItem.PushContinueItem(childItem);
                try
                {
                    decisions.AddRange(childItem.ScheduleDecisions());
                }
                finally
                {
                    _completedWorkflowItem.PopContinueItem();
                }
            }
            return decisions;
        }

        internal override bool ReadyToScheduleChildren => true;
    }
}