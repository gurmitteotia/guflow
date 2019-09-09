// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public abstract class TimerEvent : WorkflowItemEvent
    {
        private string _timerName;
        private TimeSpan _timeout;
        private long _timerStartedEventId;
        internal TimerType TimerType { get; private set; }
        protected TimerEvent(long eventId)
            : base(eventId)
        {
        }
        protected void PopulateProperties(long timerStartedEventId, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            bool foundTimerStartedEvent = false;
            _timerStartedEventId = timerStartedEventId;
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsTimerStartedEvent(timerStartedEventId))
                {
                    _timeout = TimeSpan.FromSeconds(int.Parse(historyEvent.TimerStartedEventAttributes.StartToFireTimeout));
                    ScheduleId = ScheduleId.Raw(historyEvent.TimerStartedEventAttributes.TimerId);
                    var timerScheduleData = historyEvent.TimerStartedEventAttributes.Control.As<TimerScheduleData>();
                    TimerType = timerScheduleData.TimerType;
                    _timerName = timerScheduleData.TimerName;
                    foundTimerStartedEvent = true;
                    break;
                }
            }
            if(!foundTimerStartedEvent)
                throw new IncompleteEventGraphException(string.Format("Can not find the timer started event id {0}",timerStartedEventId));
        }

        internal override bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            foreach (var timerEvent in workflowItemEvents.OfType<TimerEvent>())
            {
                if (IsInChainOf(timerEvent))
                    return true;
            }
            return false;
        }

        private bool IsInChainOf(TimerEvent otherTimerEvent)
        {
            return _timerStartedEventId == otherTimerEvent._timerStartedEventId;
        }

        internal ScheduleId Id => ScheduleId;
        internal TimeSpan Timeout => _timeout;
        public override string ToString()
        {
            return $"{GetType().Name} for timer {_timerName}, fired after {_timeout}";
        }
    }
}