// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;

namespace Guflow.Decider
{
    internal interface IWorkflow : IWorkflowDefaultActions
    {
        IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem workflowItem);
        WorkflowItem WorkflowItem(Identity identity);
        ChildWorkflowItem ChildWorkflowItem(Identity identity);
        IWorkflowHistoryEvents WorkflowHistoryEvents { get; }
        IEnumerable<WaitForSignalsEvent> WaitForSignalsEvents { get; } 
        WorkflowEvent CurrentlyExecutingEvent { get; }

        WorkflowAction WorkflowAction(WorkflowStartedEvent workflowStartedEvent);
        WorkflowAction WorkflowAction(ActivityCompletedEvent activityCompletedEvent);
        WorkflowAction WorkflowAction(ActivityFailedEvent activityFailedEvent);
        WorkflowAction WorkflowAction(ActivityTimedoutEvent activityTimedoutEvent);
        WorkflowAction WorkflowAction(ActivityCancelledEvent activityCancelledEvent);
        WorkflowAction WorkflowAction(TimerFiredEvent timerFiredEvent);
        WorkflowAction WorkflowAction(TimerStartFailedEvent timerStartFailedEvent);
        WorkflowAction WorkflowAction(ActivityCancellationFailedEvent activityCancellationFailedEvent);
        WorkflowAction WorkflowAction(TimerCancellationFailedEvent timerCancellationFailedEvent);
        WorkflowAction WorkflowAction(ActivitySchedulingFailedEvent activitySchedulingFailedEvent);
        WorkflowAction WorkflowAction(WorkflowSignaledEvent workflowSignaledEvent);
        WorkflowAction WorkflowAction(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent);
        WorkflowAction WorkflowAction(RecordMarkerFailedEvent recordMarkerFailedEvent);
        WorkflowAction WorkflowAction(WorkflowSignalFailedEvent workflowSignalFailedEvent);
        WorkflowAction WorkflowAction(WorkflowCompletionFailedEvent workflowCompletionFailedEvent);
        WorkflowAction WorkflowAction(WorkflowFailureFailedEvent workflowFailureFailedEvent);
        WorkflowAction WorkflowAction(ExternalWorkflowCancelRequestFailedEvent @event);
        WorkflowAction WorkflowAction(WorkflowCancellationFailedEvent workflowCancellationFailedEvent);
        WorkflowAction WorkflowAction(LambdaCompletedEvent lambdaCompletedEvent);
        WorkflowAction WorkflowAction(LambdaFailedEvent lamdbaFailedEvent);
        WorkflowAction WorkflowAction(LambdaTimedoutEvent lambdaTimedoutEvent);
        WorkflowAction WorkflowAction(LambdaSchedulingFailedEvent lamdbaSchedulingFailedEvent);
        WorkflowAction WorkflowAction(LambdaStartFailedEvent lambdaStartFailedEvent);
        WorkflowAction WorkflowAction(ChildWorkflowCompletedEvent completedEvent);
        WorkflowAction WorkflowAction(ChildWorkflowFailedEvent failedEvent);
        WorkflowAction WorkflowAction(ChildWorkflowCancelledEvent cancelledEvent);
        WorkflowAction WorkflowAction(ChildWorkflowTerminatedEvent terminatedEvent);
        WorkflowAction WorkflowAction(ChildWorkflowTimedoutEvent timedoutEvent);
        WorkflowAction WorkflowAction(ChildWorkflowStartFailedEvent startFailed);
        WorkflowAction WorkflowAction(WorkflowRestartFailedEvent failedEvent);
        WorkflowAction WorkflowAction(ChildWorkflowStartedEvent startedEvent);
    }
}