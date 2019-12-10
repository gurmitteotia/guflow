using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class SignalsTimedoutDetails
    {
        public string ScheduleId;
        public long TriggerEventId;
        public string[] TimedoutSignalNames;
        public string Reason;
    }
    internal sealed class SignalsTimedoutDecision : WorkflowDecision
    {
        private readonly ScheduleId _scheduleId;

        private readonly long _signalTriggerEventId;

        private readonly string[] _timedoutSignals;

        private readonly string _reason;

        public SignalsTimedoutDecision(ScheduleId scheduleId, long signalTriggerEventId, string[] timedoutSignals, string reason)
            : base(false,false)
        {
            _scheduleId = scheduleId;
            _signalTriggerEventId = signalTriggerEventId;
            _timedoutSignals = timedoutSignals;
            _reason = reason;
        }

        internal override Decision SwfDecision()
        {
            var details = new SignalsTimedoutDetails()
            {
                ScheduleId = _scheduleId.ToString(),
                TriggerEventId = _signalTriggerEventId,
                TimedoutSignalNames = _timedoutSignals,
                Reason = _reason
            };
            var attr = new RecordMarkerDecisionAttributes()
            {
                MarkerName = InternalMarkerNames.WorkflowItemSignalsTimedout,
                Details = details.ToJson()
            };
            return new Decision()
            {
                RecordMarkerDecisionAttributes = attr,
                DecisionType = DecisionType.RecordMarker
            };
        }


        private bool Equals(SignalsTimedoutDecision other)
        {
            return _scheduleId.Equals(other._scheduleId) && _signalTriggerEventId == other._signalTriggerEventId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SignalsTimedoutDecision other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_scheduleId.GetHashCode() * 397) ^ _signalTriggerEventId.GetHashCode();
            }
        }
    }
}