using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class CancelTimerDecision : WorkflowDecision
    {
        private readonly Identity _timerIdentity;
        public CancelTimerDecision(Identity timerIdentity):base(false)
        {
            _timerIdentity = timerIdentity;
        }

        internal override Decision Decision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.CancelTimer,
                CancelTimerDecisionAttributes = new CancelTimerDecisionAttributes()
                {
                    TimerId = _timerIdentity.Id.ToString()
                }
            };
        }

        private bool Equals(CancelTimerDecision other)
        {
            return _timerIdentity.Equals(other._timerIdentity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CancelTimerDecision)obj);
        }

        public override int GetHashCode()
        {
            return _timerIdentity.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} for {1}", GetType().Name, _timerIdentity);
        }
    }
}