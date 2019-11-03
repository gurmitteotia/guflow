// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Cause the workflow item to wait for the given signal(s).
    /// </summary>
    public class WorkflowItemWaitAction : WorkflowAction
    {
        private readonly ScheduleId _scheduleId;
        private readonly WaitForSignalData _data;
        private WorkflowDecision _timerDecision = WorkflowDecision.Empty;
        internal WorkflowItemWaitAction(WorkflowItemEvent itemEvent, SignalWaitType waitType, params string[] signalNames)
        {
            _scheduleId = itemEvent.ScheduleId;
            _data = new WaitForSignalData
            {
                ScheduleId = itemEvent.ScheduleId,
                TriggerEventId = itemEvent.EventId,
                WaitType = waitType,
                SignalNames = signalNames,
                NextAction = SignalNextAction.Continue
            };
        }

        internal override IEnumerable<WorkflowDecision> Decisions()
        {
            return new[] {new WaitForSignalsDecision(_data), _timerDecision};
        }

        internal override IEnumerable<WaitForSignalsEvent> WaitForSignalsEvent()
        {
            var historyEvent = SimulatedHistoryEvent();
            var @event = new WaitForSignalsEvent(historyEvent, Enumerable.Empty<HistoryEvent>());
            @event.SignalReceived += SignalReceived;
            return new[]{@event};
        }

        private void SignalReceived(WaitForSignalsEvent sender, string args)
        {
            if(!sender.IsExpectingSignals)
                _timerDecision =  WorkflowDecision.Empty;
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
        /// <summary>
        /// Wait for the signal for the given duration, if the signal are not received by the given duration workflow execution will resume. Signal APIs can be used to determine if
        /// workflow execution is resumed because of the signal or the timeout.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public WorkflowItemWaitAction For(TimeSpan timeout)
        {
            _timerDecision = ScheduleTimerDecision.SignalTimer(_scheduleId, _data.TriggerEventId ,timeout);
            return this;
        }

        internal override WorkflowAction TriggeredAction(WorkflowItem item)
        {
            if (item.IsWaitingForAnySignal()) return this;
            return item.SignalResumedAction();
        }
    }
}