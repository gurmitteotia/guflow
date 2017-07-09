using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class TimerFiredEvent :TimerEvent
    {
        internal TimerFiredEvent(HistoryEvent timerFiredEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(timerFiredEvent.EventId)
        {
            var eventAttributes =timerFiredEvent.TimerFiredEventAttributes;
            PopulateProperties(eventAttributes.StartedEventId, allHistoryEvents);
        }
        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnTimerFired(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.Continue(this);
        }
    }
}