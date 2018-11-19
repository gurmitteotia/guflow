﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class TimerCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly CancelTimerFailedEventAttributes _eventAttributes;
        internal TimerCancellationFailedEvent(HistoryEvent timerCancellationFailedEvent) : base(timerCancellationFailedEvent.EventId)
        {
            _eventAttributes = timerCancellationFailedEvent.CancelTimerFailedEventAttributes;
            ScheduleId = ScheduleId.Raw(_eventAttributes.TimerId);
        }
        public string Cause { get { return _eventAttributes.Cause; } }
        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("TIMER_CANCELLATION_FAILED", Cause);
        }
    }
}