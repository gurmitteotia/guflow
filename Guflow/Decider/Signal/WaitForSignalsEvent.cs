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
        public event EventHandler<WaitForSignalsEvent, string> SignalReceived;
        public event EventHandler<WaitForSignalsEvent> SignalTimedout;
        private SignalState _signalState;
        public WaitForSignalsEvent(HistoryEvent @event, IEnumerable<HistoryEvent> allEvents)
            : base(@event)
        {
            _data = @event.MarkerRecordedEventAttributes.Details.As<WaitForSignalData>();
            ScheduleId = ScheduleId.Raw(_data.ScheduleId);
            _signalState = new WaitingForSignalState(this);
            PopulateResumedSignals(allEvents);
        }

        private void PopulateResumedSignals(IEnumerable<HistoryEvent> allEvents)
        {
            foreach (var historyEvent in allEvents)
            {
                if (historyEvent.EventId < EventId) break;

                if (historyEvent.IsWorkflowItemSignalledEvent())
                {
                    var signalResumedEvent = historyEvent.WorkflowItemSignalledEvent();
                    if (signalResumedEvent.IsFor(this))
                        _signalState.RecordSignal(signalResumedEvent.SignalName);
                }

                if (historyEvent.IsWorkflowItemSignalTimedoutEvent())
                {
                    var signalTimedoutEvent = historyEvent.WorkflowItemSignalTimedoutEvent();
                    if (signalTimedoutEvent.IsFor(this))
                        _signalState.RecordTimedout(signalTimedoutEvent.TimeoutTriggerEventId);
                }
            }
        }
        public long TriggerEventId => _data.TriggerEventId;
        public IEnumerable<string> WaitingSignals => _signalState.WaitingSignals;

        public bool IsExpectingSignals => _signalState.IsExpectingSignals;
        public bool IsWaitingForSignal(string signalName) =>
            WaitingSignals.Contains(signalName, StringComparer.OrdinalIgnoreCase);

        public WorkflowDecision RecordSignal(WorkflowSignaledEvent signal)
        {
            _signalState.RecordSignal(signal.SignalName);
            SignalReceived?.Invoke(this, signal.SignalName);
            return new WorkflowItemSignalledDecision(ScheduleId.Raw(_data.ScheduleId), _data.TriggerEventId, signal.SignalName, signal.EventId);
        }

        public bool HasReceivedSignal(string signalName) => _signalState.HasReceivedSignal(signalName);


        public WorkflowAction NextAction(WorkflowItem workflowItem) => _signalState.NextAction(workflowItem);


        public WorkflowDecision RecordTimedout(WorkflowEvent timeoutEvent)
        {
           var decision=  _signalState.RecordTimedout(timeoutEvent.EventId);
           SignalTimedout?.Invoke(this, this);
           return decision;
        }

        public bool IsSignalTimedout(string signalName) => _signalState.IsTimedout(signalName);


        public bool HasTimedout(WorkflowSignaledEvent signal)
        {
            return _signalState.HasTimedout(signal);
        }

        public bool HasTimedoutTriggerId(long timeoutTriggerEventId) =>
            _signalState.HasTimedoutTriggerId(timeoutTriggerEventId);
       

        private abstract class SignalState
        {
            protected readonly List<string> ResumedSignals = new List<string>();

            public bool HasReceivedSignal(string signalName)
            {
                return ResumedSignals.Contains(signalName, StringComparer.OrdinalIgnoreCase);
            }

            public abstract IEnumerable<string> WaitingSignals { get; }
            public bool IsExpectingSignals => WaitingSignals.Any();

            public abstract WorkflowAction NextAction(WorkflowItem workflowItem);
            public abstract WorkflowDecision RecordTimedout(long timeoutTriggerId);
            public abstract void RecordSignal(string signalName);
            public abstract bool IsTimedout(string signalName);
            public abstract bool HasTimedout(WorkflowSignaledEvent signal);
            public abstract bool HasTimedoutTriggerId(long timeoutTriggerEventId);
        }

        private class WaitingForSignalState : SignalState
        {
            private readonly WaitForSignalsEvent _waitForSignalsEvent;
            private readonly WaitForSignalData _data;

            public WaitingForSignalState(WaitForSignalsEvent waitForSignalsEvent)
            {
                _waitForSignalsEvent = waitForSignalsEvent;
                _data = waitForSignalsEvent._data;
            }

            public override IEnumerable<string> WaitingSignals
            {
                get
                {
                    if (_data.WaitType == SignalWaitType.Any)
                        return ResumedSignals.Any() ? Enumerable.Empty<string>() : _data.SignalNames;
                    return _data.SignalNames.Except(ResumedSignals, StringComparer.OrdinalIgnoreCase);
                }
            }

            public override WorkflowDecision RecordTimedout(long timeoutTriggerId)
            {
                //TODO: Write test to ensure it works fine when it is already timedout.
                if (!IsExpectingSignals) throw new InvalidOperationException("Can't timedout non-active wait for signal event.");
                var timedoutSignals = WaitingSignals.ToList();
                _waitForSignalsEvent._signalState = new SignalTimedoutState(timedoutSignals, timeoutTriggerId);
                return new WorkflowItemSignalsTimedoutDecision(_waitForSignalsEvent.ScheduleId, _waitForSignalsEvent.TriggerEventId, WaitingSignals.ToArray(), timeoutTriggerId);
            }

            public override void RecordSignal(string signalName)
            {
                ResumedSignals.Add(signalName);
            }

            public override bool IsTimedout(string signalName) => false;
            public override bool HasTimedout(WorkflowSignaledEvent signal)
            {
                if (!_data.TriggerEventCompletionDate.HasValue) return false;
                if (!_data.Timeout.HasValue) return false;
                return (signal.Timestamp - _data.TriggerEventCompletionDate.Value) > _data.Timeout.Value;
            }

            public override bool HasTimedoutTriggerId(long timeoutTriggerEventId) => false;

            public override WorkflowAction NextAction(WorkflowItem workflowItem)
            {
                if (_data.NextAction == SignalNextAction.Continue) return WorkflowAction.ContinueWorkflow(workflowItem);
                return WorkflowAction.Schedule(workflowItem);
            }
        }

        private class SignalTimedoutState : SignalState
        {
            private readonly List<string> _timedoutSignals;
            private readonly long _timeoutTriggerId;

            public SignalTimedoutState(List<string> timedoutSignals, long timeoutTriggerId)
            {
                _timedoutSignals = timedoutSignals;
                _timeoutTriggerId = timeoutTriggerId;
            }

            public override bool IsTimedout(string signalName)
            => _timedoutSignals.Contains(signalName, StringComparer.OrdinalIgnoreCase);

            public override bool HasTimedout(WorkflowSignaledEvent signal) => true;

            public override bool HasTimedoutTriggerId(long timeoutTriggerEventId) =>
                _timeoutTriggerId == timeoutTriggerEventId;
            public override IEnumerable<string> WaitingSignals => Enumerable.Empty<string>();

            public override WorkflowAction NextAction(WorkflowItem workflowItem)
            {
                return WorkflowAction.ContinueWorkflow(workflowItem);
            }

            public override WorkflowDecision RecordTimedout(long timeoutTriggerId)
            {
                throw new InvalidOperationException("Can't record timedout when the signal is already timedout.");
            }

            public override void RecordSignal(string signalName)
            {
                throw new InvalidOperationException("Can't record signal when it is timedout.");
            }
        }
    }
}