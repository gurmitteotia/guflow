using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public sealed class ScheduleWorkflowItemAction : WorkflowAction
    {
        private readonly WorkflowItem _workflowItem;
        private WorkflowAction _scheduleWorkflowAction;
        internal ScheduleWorkflowItemAction(WorkflowItem workflowItem)
        {
            _workflowItem = workflowItem;
            _scheduleWorkflowAction = Custom(workflowItem.GetScheduleDecisions().ToArray());
        }
        public ScheduleWorkflowItemAction After(TimeSpan afterTimeout)
        {
           _scheduleWorkflowAction = Custom(_workflowItem.GetRescheduleDecision(afterTimeout));
            return this;
        }
        public WorkflowAction UpTo(Limit limit)
        {
            if (limit.IsExceeded(_workflowItem))
                _scheduleWorkflowAction = _workflowItem.DefaultActionOnLastEvent();
            return this;
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _scheduleWorkflowAction.GetDecisions();
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