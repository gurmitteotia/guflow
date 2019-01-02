// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Cause the workflow item to wait for given signals.
    /// </summary>
    public class WaitForSignalsWorkflowAction : WorkflowAction
    {
        private readonly ScheduleId _scheduleId;
        private readonly long _triggerEventId;
        private readonly SignalWaitType _waitType;
        private readonly string[] _signalNames;
        private WaitForSignalsEvent _generatedEvent;
        internal WaitForSignalsWorkflowAction(ScheduleId scheduleId, long triggerEventId, SignalWaitType waitType, params string[] signalNames)
        {
            _scheduleId = scheduleId;
            _triggerEventId = triggerEventId;
            _waitType = waitType;
            _signalNames = signalNames;
        }

        internal override IEnumerable<WorkflowDecision> Decisions()
        {
            return new[] {new WaitForSignalsDecision(_scheduleId, _triggerEventId, _signalNames)};
        }

        internal override IEnumerable<WaitForSignalsEvent> WaitForSignalsEvent()
        {
            if (_generatedEvent != null) return new []{_generatedEvent};
            var historyEvent = SimulatedHistoryEvent();
            return new[]{_generatedEvent = new WaitForSignalsEvent(historyEvent, Enumerable.Empty<HistoryEvent>())};
        }

        private HistoryEvent SimulatedHistoryEvent()
        {
            var data = new WaitForSignalScheduleData()
            {
                ScheduleId = _scheduleId,
                TriggerEventId = _triggerEventId,
                SignalNames = _signalNames,
                WaitType = _waitType
            };
            var historyEvent = new HistoryEvent();
            historyEvent.EventId = long.MaxValue - data.TriggerEventId;
            historyEvent.EventType = EventType.MarkerRecorded;
            var attr = new MarkerRecordedEventAttributes();
            attr.MarkerName = InternalMarkerNames.WorkflowItemWaitForSignals;
            attr.Details = data.ToJson();
            historyEvent.MarkerRecordedEventAttributes = attr;
            return historyEvent;
        }

    }
}