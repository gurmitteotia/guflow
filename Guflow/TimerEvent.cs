using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class TimerEvent : WorkflowItemEvent
    {
        protected AwsIdentity TimerIdentity;
        protected bool IsARescheduledTimer;

        public string Name { get; private set; }
        public TimeSpan FiredAfter { get; private set; }


        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(TimerIdentity) && !IsARescheduledTimer;
        }
        internal bool IsRescheduleTimerFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(TimerIdentity) && IsARescheduledTimer;
        }
        protected void PopulateProperties(long timerStartedEventId, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            bool foundTimerStartedEvent = false;
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsTimerStartedEventFor(timerStartedEventId))
                {
                    FiredAfter = TimeSpan.FromSeconds(int.Parse(historyEvent.TimerStartedEventAttributes.StartToFireTimeout));
                    TimerIdentity = AwsIdentity.Raw(historyEvent.TimerStartedEventAttributes.TimerId);
                    var timerScheduleData = historyEvent.TimerStartedEventAttributes.Control.FromJson<TimerScheduleData>();
                    IsARescheduledTimer = timerScheduleData.IsARescheduleTimer;
                    Name = timerScheduleData.TimerName;
                    foundTimerStartedEvent = true;
                    break;
                }
            }
            if(!foundTimerStartedEvent)
                throw new IncompleteEventGraphException(string.Format("Can not find the timer started event id {0}",timerStartedEventId));
        }
    }
}