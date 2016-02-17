using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerFiredEvent :WorkflowEvent
    {
        private readonly TimerFiredEventAttributes _eventAttributes;
        public TimerFiredEvent(HistoryEvent timerFiredEvent, IEnumerable<HistoryEvent> allHisotryEvents)
        {
            _eventAttributes =timerFiredEvent.TimerFiredEventAttributes;
            PopulateProperties(allHisotryEvents);
        }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerFired(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { throw new System.NotImplementedException(); }
        }

        public string Name { get { return _eventAttributes.TimerId; } }
        public TimeSpan FireAfter { get; private set; }


        private void PopulateProperties(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsTimerStartedEventFor(_eventAttributes.StartedEventId))
                {
                    FireAfter = TimeSpan.FromSeconds(int.Parse(historyEvent.TimerStartedEventAttributes.StartToFireTimeout));
                }
            }
        }
    }
}