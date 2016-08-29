using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal sealed class CancelRequestWorkflowDecision : WorkflowDecision
    {
        private readonly string _workflowId;
        private readonly string _runId;

        public CancelRequestWorkflowDecision(string workflowId, string runId)
            : base(false)
        {
            _workflowId = workflowId;
            _runId = runId;
        }

        internal override Decision Decision()
        {
            throw new System.NotImplementedException();
        }

        private bool Equals(CancelRequestWorkflowDecision other)
        {
            return string.Equals(_workflowId, other._workflowId) && string.Equals(_runId, other._runId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CancelRequestWorkflowDecision)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_workflowId.GetHashCode() * 397) ^ (_runId == null ? 10 : _runId.GetHashCode());
            }
        }

    }
}