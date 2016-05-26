using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerCancelledEvent : TimerEvent
    {
        public TimerCancelledEvent(HistoryEvent timerCancelledEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            var eventAttributes = timerCancelledEvent.TimerCanceledEventAttributes;
            PopulateProperties(eventAttributes.StartedEventId, allHistoryEvents);
        }
        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerCancelled(this);
        }
    }
}