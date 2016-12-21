using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    public sealed class ScheduleWorkflowItemAction : WorkflowAction
    {
        private readonly WorkflowItem _workflowItem;
        private readonly WorkflowAction _workflowAction;
        internal ScheduleWorkflowItemAction(WorkflowItem workflowItem)
        {
            _workflowItem = workflowItem;
            _workflowAction = new GenericWorkflowAction(_workflowItem.GetScheduleDecision());
        }

        public WorkflowAction After(TimeSpan afterTimeout)
        {
            return new GenericWorkflowAction(_workflowItem.GetRescheduleDecision(afterTimeout));
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _workflowAction.GetDecisions();
        }
        private bool Equals(ScheduleWorkflowItemAction other)
        {
            return _workflowItem.Equals(other._workflowItem);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ScheduleWorkflowItemAction && Equals((ScheduleWorkflowItemAction)obj);
        }

        public override int GetHashCode()
        {
            return _workflowItem.GetHashCode();
        }
    }
}