using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    public sealed class JumpWorkflowAction : WorkflowAction
    {
        private readonly ScheduleWorkflowItemAction _scheduleAction;

        internal JumpWorkflowAction(WorkflowItem workflowItem)
        {
            _scheduleAction = new ScheduleWorkflowItemAction(workflowItem);
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _scheduleAction.GetDecisions();
        }

        public WorkflowAction After(TimeSpan interval)
        {
            return _scheduleAction.After(interval);
        }
        private bool Equals(JumpWorkflowAction other)
        {
            return _scheduleAction.Equals(other._scheduleAction);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JumpWorkflowAction)obj);
        }

        public override int GetHashCode()
        {
            return _scheduleAction.GetHashCode();
        }

    }
}