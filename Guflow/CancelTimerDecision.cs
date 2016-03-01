using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal sealed class CancelTimerDecision : WorkflowDecision
    {
        private readonly Identity _timerIdentity;
        public CancelTimerDecision(Identity timerIdentity)
        {
            _timerIdentity = timerIdentity;
        }

        public override Decision Decision()
        {
            throw new System.NotImplementedException();
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
    }
}