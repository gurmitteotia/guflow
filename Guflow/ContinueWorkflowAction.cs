using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    internal class ContinueWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _completedWorkflowItem;
        private const string _defaultWorkflowCompletedResult = "Workflow is completed.";
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
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            var childItems = _completedWorkflowItem.GetChildlern();
            if(!childItems.Any())
                return new[]{new CompleteWorkflowDecision(_defaultWorkflowCompletedResult,true)};
           
            var schedulableChildItems = childItems.Where(s => s.SchedulingIsAllowedByAllParents());
            return schedulableChildItems.Select(f => f.GetScheduleDecision());
        }

        internal override bool AllowSchedulingOfChildWorkflowItem()
        {
            return true;
        }
    }
}