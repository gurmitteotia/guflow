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
        private TimeSpan? _timerWait = null;
        private DateTime _waitingEventTimeStamp;
        internal WorkflowItemWaitAction(WorkflowItemEvent itemEvent, SignalWaitType waitType, params string[] signalNames)
        {
            _scheduleId = itemEvent.ScheduleId;
            _waitingEventTimeStamp = itemEvent.Timestamp;
            _data = new WaitForSignalData
            {
                ScheduleId = itemEvent.ScheduleId,
                TriggerEventId = itemEvent.EventId,
                WaitType = waitType,
                SignalNames = signalNames,
                NextAction = SignalNextAction.Continue,
                TriggerEventCompletionDate = itemEvent.Timestamp
            };
        }

        internal override IEnumerable<WorkflowDecision> Decisions(IWorkflow workflow)
        {
            return new[] {new WaitForSignalsDecision(_data), WaitForSignalTimerDecision(workflow.WorkflowHistoryEvents)};
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
            if (!sender.IsExpectingSignals)
                _timerWait = null;
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

        private WorkflowDecision WaitForSignalTimerDecision(IWorkflowHistoryEvents historyEvents)
        {
            if(_timerWait==null) return WorkflowDecision.Empty;

            var delay = historyEvents.ServerTimeUtc - _waitingEventTimeStamp;
            if(delay > _timerWait.Value) return WorkflowDecision.Empty;
            var timeout = _timerWait.Value - delay;
            return ScheduleTimerDecision.SignalTimer(_scheduleId, _data.TriggerEventId, timeout);
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
            _timerWait = timeout;
            _data.Timeout = timeout;
            return this;
        }

        internal override WorkflowAction TriggeredAction(WorkflowItem item)
        {
            if (item.IsWaitingForAnySignal()) return this;
            return item.SignalResumedAction();
        }
    }
}