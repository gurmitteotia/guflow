using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerFiredEvent :TimerEvent
    {
        public TimerFiredEvent(HistoryEvent timerFiredEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(timerFiredEvent.EventId)
        {
            var eventAttributes =timerFiredEvent.TimerFiredEventAttributes;
            PopulateProperties(eventAttributes.StartedEventId, allHistoryEvents);
        }
        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerFired(this);
        }
    }
}