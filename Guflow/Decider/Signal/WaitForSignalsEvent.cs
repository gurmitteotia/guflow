// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal delegate void EventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs args);

    internal class WaitForSignalsEvent : WorkflowItemEvent
    {
        private readonly WaitForSignalData _data;
        private readonly List<string> _resumedSignals = new List<string>();
        public event EventHandler<WaitForSignalsEvent, string> SignalReceived;
        private List<string> _timedoutSignals = new List<string>();
        public WaitForSignalsEvent(HistoryEvent @event, IEnumerable<HistoryEvent> allEvents)
            : base(@event)
        {
            _data = @event.MarkerRecordedEventAttributes.Details.As<WaitForSignalData>();
            ScheduleId = ScheduleId.Raw(_data.ScheduleId);
            PopulateResumedSignals(allEvents);
        }

        private void PopulateResumedSignals(IEnumerable<HistoryEvent> allEvents)
        {
            foreach (var historyEvent in allEvents)
            {
                if (historyEvent.EventId < EventId) break;
                var signalResumedEvent = historyEvent.WorkflowItemSignalledEvent();
                if(signalResumedEvent == null) continue;
                if(signalResumedEvent.IsFor(this))
                    _resumedSignals.Add(signalResumedEvent.SignalName);
            }
        }
        public long TriggerEventId => _data.TriggerEventId;
        public IEnumerable<string> WaitingSignals
        {
            get
            {
                if (_data.WaitType == SignalWaitType.Any)
                    return _resumedSignals.Any() ? Enumerable.Empty<string>() : _data.SignalNames;
                return _data.SignalNames.Except(_resumedSignals, StringComparer.OrdinalIgnoreCase);
            }
        }
        public bool IsExpectingSignals => WaitingSignals.Any();
        public bool IsWaitingForSignal(string signalName) =>
            WaitingSignals.Contains(signalName, StringComparer.OrdinalIgnoreCase);

        public WorkflowDecision RecordSignal(string signalName, long signalEventId)
        {
            _resumedSignals.Add(signalName);
            SignalReceived?.Invoke(this, signalName);
            return new WorkflowItemSignalledDecision(ScheduleId.Raw(_data.ScheduleId), _data.TriggerEventId, signalName, signalEventId);
        }

        public bool HasReceivedSignal(string signalName)
        {
            return _resumedSignals.Contains(signalName, StringComparer.OrdinalIgnoreCase);
        }

        public WorkflowAction NextAction(WorkflowItem workflowItem)
        {
            if (_data.NextAction == SignalNextAction.Continue) return WorkflowAction.ContinueWorkflow(workflowItem);
            return WorkflowAction.Schedule(workflowItem);
        }

        public WorkflowDecision RecordTimedout(string reason)
        {
            //TODO: Write test to ensure it works fine when it is already timedout.
            if(!IsExpectingSignals) throw new InvalidOperationException("Can't timedout non-active wait for signal event.");
            _timedoutSignals = WaitingSignals.ToList();
            return new SignalsTimedoutDecision(ScheduleId, TriggerEventId, WaitingSignals.ToArray(), reason);
        }

        public bool IsTimedout(string signalName)
        {
            return _timedoutSignals.Contains(signalName, StringComparer.OrdinalIgnoreCase);
        }
    }
}