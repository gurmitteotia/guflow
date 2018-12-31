// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WaitForSignalsEvent : WorkflowItemEvent
    {
        private readonly WaitForSignalScheduleData _data;
        private readonly List<string> _resumedSignals = new List<string>();

        public WaitForSignalsEvent(HistoryEvent @event, IEnumerable<HistoryEvent> allEvents) : base(@event.EventId)
        {
            _data = @event.MarkerRecordedEventAttributes.Details.As<WaitForSignalScheduleData>();
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

        /// <summary>
        /// Returns the names of signals it is currently waiting for. 
        /// </summary>
        public IEnumerable<string> WaitingSignals
        {
            get
            {
                if (_data.WaitType == SignalWaitType.Any)
                    return _resumedSignals.Any() ? Enumerable.Empty<string>() : _data.SignalNames;
                return _data.SignalNames.Except(_resumedSignals);
            }
        }

        /// <summary>
        /// Return true if it waiting for more signals.
        /// </summary>
        public bool IsExpectingSignal => WaitingSignals.Any();

        /// <summary>
        /// Returns true if waiting for given signal.
        /// </summary>
        /// <param name="signalName"></param>
        /// <returns></returns>
        public bool IsWaitingForSignal(string signalName) =>
            WaitingSignals.Contains(signalName, StringComparer.OrdinalIgnoreCase);

    }
}