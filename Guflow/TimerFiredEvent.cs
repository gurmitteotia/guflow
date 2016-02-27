using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerFiredEvent :WorkflowItemEvent
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly TimerFiredEventAttributes _eventAttributes;
        private Identity _timerIdentity;
        private bool _isARescheduledTimer;
        public TimerFiredEvent(HistoryEvent timerFiredEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
            _eventAttributes =timerFiredEvent.TimerFiredEventAttributes;
            PopulateProperties(allHistoryEvents);
        }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerFired(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { return new WorkflowContext(_allHistoryEvents); }
        }

        public string Name { get { return _timerIdentity.Name; } }
        public TimeSpan FiredAfter { get; private set; }

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_timerIdentity) && !_isARescheduledTimer;
        }

        internal bool IsRescheduleTimerFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_timerIdentity) && _isARescheduledTimer;
        }

        private void PopulateProperties(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsTimerStartedEventFor(_eventAttributes.StartedEventId))
                {
                    FiredAfter = TimeSpan.FromSeconds(int.Parse(historyEvent.TimerStartedEventAttributes.StartToFireTimeout));
                    var timerScheduleData = historyEvent.TimerStartedEventAttributes.Control.FromJson<TimerScheduleData>();
                    _timerIdentity = timerScheduleData.Identity.FromJson();
                    _isARescheduledTimer = timerScheduleData.IsARescheduleTimer;
                }
            }
        }
    }
}