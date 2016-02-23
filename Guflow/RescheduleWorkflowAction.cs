using System.Collections.Generic;

namespace Guflow
{
    internal sealed class RescheduleWorkflowAction : WorkflowAction
    {
        public bool Equals(RescheduleWorkflowAction other)
        {
            return _workflowItem.Equals(other._workflowItem);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RescheduleWorkflowAction)obj);
        }

        public override int GetHashCode()
        {
            return _workflowItem.GetHashCode();
        }

        private readonly WorkflowItem _workflowItem;

        public RescheduleWorkflowAction(WorkflowItem workflowItem)
        {
            _workflowItem = workflowItem;
        }


        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {_workflowItem.GetDecision()};
        }
    }
}