using System;
using System.Collections.Generic;

namespace Guflow
{
    internal class CancelWorkflowAction : WorkflowAction
    {
        private readonly string _details;

        public CancelWorkflowAction(string details)
        {
            _details = details;
        }

        public override bool Equals(object other)
        {
            var otherAction = other as CancelWorkflowAction;
            if (otherAction == null)
                return false;

            return string.Equals(_details, otherAction._details, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(_details) ? GetType().GetHashCode() : _details.GetHashCode();
        }

        public override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return new[] {new CancelWorkflowDecision(_details),};
        }
    }
}