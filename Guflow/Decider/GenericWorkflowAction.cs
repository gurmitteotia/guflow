using System.Collections.Generic;

namespace Guflow.Decider
{
    internal sealed class GenericWorkflowAction : WorkflowAction
    {
        private readonly WorkflowDecision _workflowDecision;
        public GenericWorkflowAction(WorkflowDecision workflowDecision)
        {
            _workflowDecision = workflowDecision;
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] { _workflowDecision };
        }
        private bool Equals(GenericWorkflowAction other)
        {
            return _workflowDecision.Equals(other._workflowDecision);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GenericWorkflowAction)obj);
        }
        public override int GetHashCode()
        {
            return _workflowDecision.GetHashCode();
        }

        public override string ToString()
        {
            return _workflowDecision.ToString();
        }
    }
}