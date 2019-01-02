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
        public WaitForSignalsDecision(ScheduleId id, long eventId, string signalName) : base(false)
        {
            _data = new WaitForSignalScheduleData()
            {
                ScheduleId = id,
                TriggerEventId = eventId,
                SignalNames = new[] { signalName },
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

        internal WaitForSignalsEvent WaitForSignalsEvent()
        {
            var historyEvent = SimulatedHistoryEvent();
            return new WaitForSignalsEvent(historyEvent, Enumerable.Empty<HistoryEvent>());
        }

        private HistoryEvent SimulatedHistoryEvent()
        {
            var historyEvent = new HistoryEvent();
            historyEvent.EventId = long.MaxValue - _data.TriggerEventId;
            historyEvent.EventType = EventType.MarkerRecorded;
            var attr = new MarkerRecordedEventAttributes();
            attr.MarkerName = InternalMarkerNames.WorkflowItemWaitForSignals;
            attr.Details = _data.ToJson();
            historyEvent.MarkerRecordedEventAttributes = attr;
            return historyEvent;
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