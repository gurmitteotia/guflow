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
    public class WorkflowItemWaitAction : WorkflowAction
    {
        private readonly WaitForSignalData _data;

        internal WorkflowItemWaitAction(ScheduleId scheduleId, long triggerEventId, SignalWaitType waitType, params string[] signalNames)
        {
            _data = new WaitForSignalData
            {
                ScheduleId = scheduleId,
                TriggerEventId = triggerEventId,
                WaitType = waitType,
                SignalNames = signalNames,
                NextAction = SignalNextAction.Continue
            };
        }

        internal override IEnumerable<WorkflowDecision> Decisions()
        {
            return new[] {new WaitForSignalsDecision(_data)};
        }

        internal override IEnumerable<WaitForSignalsEvent> WaitForSignalsEvent()
        {
            var historyEvent = SimulatedHistoryEvent();
            return new[]{new WaitForSignalsEvent(historyEvent, Enumerable.Empty<HistoryEvent>())};
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

        /// <summary>
        /// Reschedule the waiting item on receiving the necessary signals.
        /// </summary>
        /// <returns></returns>
        public WorkflowItemWaitAction ToReschedule()
        {
            _data.NextAction = SignalNextAction.Reschedule;
            return this;
        }

        internal override WorkflowAction TriggeredAction(WorkflowItem item)
        {
            if (item.IsWaitingForAnySignal()) return this;
            return item.SignalResumedAction();
        }
    }
}