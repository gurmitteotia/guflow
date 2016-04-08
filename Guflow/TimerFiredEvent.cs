using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerFiredEvent :TimerEvent
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        public TimerFiredEvent(HistoryEvent timerFiredEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
            var eventAttributes =timerFiredEvent.TimerFiredEventAttributes;
            PopulateProperties(eventAttributes.StartedEventId, allHistoryEvents);
        }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerFired(this);
        }

        public override IWorkflowHistoryEvents WorkflowHistoryEvents
        {
            get { return new WorkflowHistoryEvents(_allHistoryEvents); }
        }
    }
}