// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Amazon.SimpleWorkflow.Model;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;

namespace Guflow.Decider
{
    internal class WaitForSignalsDecision: WorkflowDecision
    {
     
        private readonly WaitForSignalScheduleData _data;
        public WaitForSignalsDecision(ScheduleId id, long eventId, params string[] signalNames) : base(false)
        {
            _data = new WaitForSignalScheduleData()
            {
                ScheduleId = id,
                SignalNames = signalNames,
                TriggerEventId = eventId,
                WaitType = SignalWaitType.Any,
                NextAction = SignalNextAction.Continue
            };
        }

        internal override Decision SwfDecision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.RecordMarker,
                RecordMarkerDecisionAttributes = new RecordMarkerDecisionAttributes()
                {
                    MarkerName = InternalMarkerNames.WorkflowItemWaitForSignals,
                    Details = _data.ToJson()
                }
            };
        }

        public override bool Equals(object obj)
        {
            return obj is WaitForSignalsDecision decision &&
                   string.Equals(_data.ScheduleId, decision._data.ScheduleId) &&
                   _data.TriggerEventId == decision._data.TriggerEventId;
        }

        public override int GetHashCode()
        {
            var hashCode = -1394181897;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_data.ScheduleId);
            hashCode = hashCode * -1521134295 + _data.TriggerEventId.GetHashCode();
            return hashCode;
        }
    }
}