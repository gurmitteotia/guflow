// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class TimerStartFailedEvent : WorkflowItemEvent
    {
        private readonly StartTimerFailedEventAttributes _startTimerFailedAttributes;
        internal TimerStartFailedEvent(HistoryEvent startTimerFailedEvent) : base(startTimerFailedEvent.EventId)
        {
            _startTimerFailedAttributes = startTimerFailedEvent.StartTimerFailedEventAttributes;
            SwfIdentity =  SwfIdentity.Raw(_startTimerFailedAttributes.TimerId);
        }

        internal string Cause { get { return _startTimerFailedAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("TIMER_START_FAILED", Cause);
        }
    }
}