using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class TimerCancelledEvent : TimerEvent
    {
        public TimerCancelledEvent(HistoryEvent timerCancelledEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(timerCancelledEvent.EventId)
        {
            var eventAttributes = timerCancelledEvent.TimerCanceledEventAttributes;
            PopulateProperties(eventAttributes.StartedEventId, allHistoryEvents);
        }
        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnTimerCancelled(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.CancelWorkflow("TIMER_CANCELLED");
        }
    }
}