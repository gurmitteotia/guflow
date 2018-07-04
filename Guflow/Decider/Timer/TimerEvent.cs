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
        private TimeSpan _firedAfter;
        private long _timerStartedEventId;
        internal bool IsARescheduledTimer { get; private set; }

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
                    _firedAfter = TimeSpan.FromSeconds(int.Parse(historyEvent.TimerStartedEventAttributes.StartToFireTimeout));
                    AwsIdentity = AwsIdentity.Raw(historyEvent.TimerStartedEventAttributes.TimerId);
                    var timerScheduleData = historyEvent.TimerStartedEventAttributes.Control.FromJson<TimerScheduleData>();
                    IsARescheduledTimer = timerScheduleData.IsARescheduleTimer;
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

            //swf does not link cancellation failed event with started event id.
            foreach (var itemEvent in workflowItemEvents.OfType<TimerCancellationFailedEvent>())
            {
                if (itemEvent.IsForSameWorkflowItemAs(this))
                    return true;
            }
            return false;
        }

        private bool IsInChainOf(TimerEvent otherTimerEvent)
        {
            return _timerStartedEventId == otherTimerEvent._timerStartedEventId;
        }
        public override string ToString()
        {
            return string.Format("{0} for timer {1}, fired after {2}",GetType().Name,_timerName,_firedAfter);
        }
    }
}