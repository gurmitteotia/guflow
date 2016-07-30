using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal sealed class RecordMarkerDecision : WorkflowDecision
    {
        private readonly string _markerName;
        private readonly string _details;

        public RecordMarkerDecision(string markerName,string details) : base(false, false)
        {
            _markerName = markerName;
            _details = details;
        }

        internal override Decision Decision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.RecordMarker,
                RecordMarkerDecisionAttributes = new RecordMarkerDecisionAttributes()
                {
                    MarkerName = _markerName,
                    Details = _details
                }
            };
        }

        private bool Equals(RecordMarkerDecision other)
        {
            return string.Equals(_markerName, other._markerName) && string.Equals(_details, other._details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RecordMarkerDecision && Equals((RecordMarkerDecision)obj);
        }

        public override int GetHashCode()
        {
            return _markerName.GetHashCode() ^ (_details != null ? _details.GetHashCode() : 0);
        }
    }
}