using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    internal class ContinueWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _completedWorkflowItem;
        private readonly IWorkflowHistoryEvents _workflowHistoryEvents;

        public ContinueWorkflowAction(WorkflowItem completedWorkflowItem, IWorkflowHistoryEvents workflowHistoryEvents)
        {
            _completedWorkflowItem = completedWorkflowItem;
            _workflowHistoryEvents = workflowHistoryEvents;
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

            var schedulableChildItems = childItems.Where(s => s.AllParentsAreProcessed(_workflowHistoryEvents));

            return schedulableChildItems.Select(f => f.GetScheduleDecision());
        }
    }
}