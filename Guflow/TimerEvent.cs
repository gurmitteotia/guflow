using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class TimerEvent : WorkflowItemEvent
    {
        private string _timerName;
        private TimeSpan _firedAfter;
        internal bool IsARescheduledTimer { get; private set; }
        protected void PopulateProperties(long timerStartedEventId, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            bool foundTimerStartedEvent = false;
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsTimerStartedEventFor(timerStartedEventId))
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

        public override string ToString()
        {
            return string.Format("{0} for timer {1}, fired after {2}",GetType().Name,_timerName,_firedAfter);
        }
    }
}