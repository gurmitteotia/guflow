using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class CancelActivityDecision : WorkflowDecision
    {
        private readonly Identity _activityIdentiy;

        public CancelActivityDecision(Identity activityIdentiy):base(false)
        {
            _activityIdentiy = activityIdentiy;
        }

        internal override Decision Decision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.RequestCancelActivityTask,
                RequestCancelActivityTaskDecisionAttributes = new RequestCancelActivityTaskDecisionAttributes()
                {
                    ActivityId = _activityIdentiy.Id.ToString()
                }
            };
        }
        private bool Equals(CancelActivityDecision other)
        {
            return _activityIdentiy.Equals(other._activityIdentiy);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CancelActivityDecision && Equals((CancelActivityDecision)obj);
        }

        public override int GetHashCode()
        {
            return _activityIdentiy.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} for {1}", GetType().Name, _activityIdentiy);
        }
    }
}