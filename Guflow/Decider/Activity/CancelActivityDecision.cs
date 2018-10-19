// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class CancelActivityDecision : WorkflowDecision
    {
        private readonly Identity _identity;

        public CancelActivityDecision(Identity identity):base(false)
        {
            _identity = identity;
        }

        internal override Decision SwfDecision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.RequestCancelActivityTask,
                RequestCancelActivityTaskDecisionAttributes = new RequestCancelActivityTaskDecisionAttributes()
                {
                    ActivityId = _identity.Id.ToString(),
                }
            };
        }
        private bool Equals(CancelActivityDecision other)
        {
            return _identity.Equals(other._identity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CancelActivityDecision && Equals((CancelActivityDecision)obj);
        }

        public override int GetHashCode()
        {
            return _identity.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} for {1}", GetType().Name, _identity);
        }
    }
}