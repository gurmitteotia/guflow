using System;
using System.Collections.Generic;

namespace Guflow
{
    public sealed class RescheduleWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _workflowItem;
        private readonly WorkflowAction _workflowAction;
        public RescheduleWorkflowAction(WorkflowItem workflowItem)
        {
            _workflowItem = workflowItem;
            _workflowAction = new GenericWorkflowAction(_workflowItem.GetDecision());
        }

        public WorkflowAction After(TimeSpan afterTimeout)
        {
            return new GenericWorkflowAction(_workflowItem.GetRescheduleDecision(afterTimeout));
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _workflowAction.GetDecisions();
        }
        private bool Equals(RescheduleWorkflowAction other)
        {
            return _workflowItem.Equals(other._workflowItem);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RescheduleWorkflowAction && Equals((RescheduleWorkflowAction)obj);
        }

        public override int GetHashCode()
        {
            return _workflowItem.GetHashCode();
        }

    }
}