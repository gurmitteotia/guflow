using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerCancelledEvent : TimerEvent
    {
        public TimerCancelledEvent(HistoryEvent timerCancelledEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(timerCancelledEvent.EventId)
        {
            var eventAttributes = timerCancelledEvent.TimerCanceledEventAttributes;
            PopulateProperties(eventAttributes.StartedEventId, allHistoryEvents);
        }
        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnTimerCancelled(this);
        }
    }
}