// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;

namespace Guflow.Decider
{
    internal interface IWorkflowHistoryEvents
    {
        WorkflowItemEvent LastActivityEvent(ActivityItem activityItem);
        WorkflowItemEvent LastTimerEvent(TimerItem timerItem, bool includeRescheduleTimerEvents);
        IEnumerable<WorkflowEvent> NewEvents();
        WorkflowStartedEvent WorkflowStartedEvent();
        bool HasActiveEvent();
        long LatestEventId { get; }
        string WorkflowRunId { get; }
        IEnumerable<WorkflowItemEvent> AllActivityEvents(ActivityItem activityItem);
        IEnumerable<WorkflowItemEvent> AllTimerEvents(TimerItem timerItem, bool includeRescheduleTimerEvents);
        IEnumerable<MarkerRecordedEvent> AllMarkerRecordedEvents();
        IEnumerable<WorkflowSignaledEvent> AllSignalEvents();
        IEnumerable<WorkflowCancellationRequestedEvent> AllWorkflowCancellationRequestedEvents();
        IEnumerable<WorkflowItemEvent> AllLambdaEvents(LambdaItem lambdaItem);
        WorkflowItemEvent LastLambdaEvent(LambdaItem lambdaItem);
        IEnumerable<WorkflowItemEvent> AllChildWorkflowEvents(ChildWorkflowItem childWorkflowItem);
        WorkflowItemEvent LastChildWorkflowEvent(ChildWorkflowItem childWorkflowItem);
        IEnumerable<WorkflowItem> WaitingItems(WorkflowItem[] workflowItems, string signalName);
        WaitForSignalsEvent SignalWaitEvent(WorkflowItem workflowItem, string signalName);
    }
}