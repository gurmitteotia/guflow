using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public sealed class JumpWorkflowAction : WorkflowAction
    {
        private  WorkflowAction _triggeredAction;
        private readonly ScheduleWorkflowItemAction _scheduleAction;

        internal JumpWorkflowAction(WorkflowItem jumpToItem)
        {
            _scheduleAction = new ScheduleWorkflowItemAction(jumpToItem);
            _triggeredAction = Empty;
        }
        internal JumpWorkflowAction SetTriggerAction(WorkflowAction workflowAction)
        {
            _triggeredAction = workflowAction;
            return this;
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _scheduleAction.GetDecisions().Concat(_triggeredAction.GetDecisions());
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