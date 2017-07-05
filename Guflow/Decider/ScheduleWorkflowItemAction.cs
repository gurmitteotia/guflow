using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    public sealed class ScheduleWorkflowItemAction : WorkflowAction
    {
        private readonly WorkflowItem _workflowItem;
        internal ScheduleWorkflowItemAction(WorkflowItem workflowItem)
        {
            _workflowItem = workflowItem;
        }

        public WorkflowAction After(TimeSpan afterTimeout)
        {
            return new GenericWorkflowAction(_workflowItem.GetRescheduleDecision(afterTimeout));
        }

        public WorkflowAction UpTo(Limit limit)
        {
            return new ScheduleWorkflowItemAction(_workflowItem);
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {_workflowItem.GetScheduleDecision()};
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