using System.Collections.Generic;

namespace Guflow
{
    internal class CompleteWorkflowAction: WorkflowAction
    {
        private readonly string _result;

        public CompleteWorkflowAction(string result)
        {
            _result = result;
        }

        public override bool Equals(object other)
        {
            var otherAction = other as CompleteWorkflowAction;
            if (otherAction == null)
                return false;
            return string.Equals(_result, otherAction._result);
        }

        public override int GetHashCode()
        {
            return _result == null ? GetType().GetHashCode() : _result.GetHashCode();
        }

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {new CompleteWorkflowDecision(_result),};
        }
    }
}