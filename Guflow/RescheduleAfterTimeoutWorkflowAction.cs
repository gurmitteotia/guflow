using System;
using System.Collections.Generic;

namespace Guflow
{
    internal sealed class RescheduleAfterTimeoutWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _workflowItem;
        private readonly TimeSpan _timeout;

        public RescheduleAfterTimeoutWorkflowAction(WorkflowItem workflowItem, TimeSpan timeout)
        {
            _workflowItem = workflowItem;
            _timeout = timeout;
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {_workflowItem.GetRescheduleDecision(_timeout)};
        }
        public bool Equals(RescheduleAfterTimeoutWorkflowAction other)
        {
            return _workflowItem.Equals(other._workflowItem) && _timeout.Equals(other._timeout);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RescheduleAfterTimeoutWorkflowAction)obj);
        }
        public override int GetHashCode()
        {
            return _workflowItem.GetHashCode();
        }
    }
}