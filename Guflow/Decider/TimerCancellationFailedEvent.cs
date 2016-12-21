﻿using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class TimerCancellationFailedEvent : WorkflowItemEvent
    {
        private readonly CancelTimerFailedEventAttributes _eventAttributes;
        internal TimerCancellationFailedEvent(HistoryEvent timerCancellationFailedEvent) : base(timerCancellationFailedEvent.EventId)
        {
            _eventAttributes = timerCancellationFailedEvent.CancelTimerFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(_eventAttributes.TimerId);
        }
        public string Cause { get { return _eventAttributes.Cause; } }
        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnTimerCancellationFailed(this);
        }
    }
}