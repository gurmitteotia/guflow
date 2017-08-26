using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal sealed class IgnoreWorkflowAction : WorkflowAction
    {
        private readonly bool _keepBranchActive;

        public IgnoreWorkflowAction(bool keepBranchActive)
        {
            _keepBranchActive = keepBranchActive;
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return Enumerable.Empty<WorkflowDecision>();
        }

        internal override bool CanScheduleAny(IEnumerable<WorkflowItem> workflowItems)
        {
            return _keepBranchActive;
        }

        internal override bool ReadyToScheduleChildren
        {
            get { return !_keepBranchActive; }
        }

        private bool Equals(IgnoreWorkflowAction other)
        {
            return _keepBranchActive == other._keepBranchActive;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IgnoreWorkflowAction)obj);
        }
        public override int GetHashCode()
        {
            return _keepBranchActive.GetHashCode();
        }

    }
}