// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class RecordMarkerWorkflowDecision : WorkflowDecision
    {
        private readonly string _markerName;
        private readonly string _details;

        public RecordMarkerWorkflowDecision(string markerName,string details) : base(false, false)
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

        private bool Equals(RecordMarkerWorkflowDecision other)
        {
            return string.Equals(_markerName, other._markerName) && string.Equals(_details, other._details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RecordMarkerWorkflowDecision && Equals((RecordMarkerWorkflowDecision)obj);
        }

        public override int GetHashCode()
        {
            return _markerName.GetHashCode() ^ (_details != null ? _details.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return String.Format("Marker name {0}, details {1}",_markerName,_details);
        }
    }
}