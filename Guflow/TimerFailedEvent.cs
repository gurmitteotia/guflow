using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerFailedEvent : WorkflowEvent
    {
        private readonly StartTimerFailedEventAttributes _startTimerFailedAttributes;
        public TimerFailedEvent(HistoryEvent startTimerFailedEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _startTimerFailedAttributes = startTimerFailedEvent.StartTimerFailedEventAttributes;
        }

        internal string Cause { get { return _startTimerFailedAttributes.Cause; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerFailed(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { throw new System.NotImplementedException(); }
        }

    }
}