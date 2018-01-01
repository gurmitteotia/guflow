using System.Collections.Generic;

namespace Guflow.Decider
{
    internal interface IWorkflowHistoryEvents
    {
        WorkflowItemEvent LatestActivityEventFor(ActivityItem activityItem);
        WorkflowItemEvent LatestTimerEventFor(TimerItem timerItem);
        IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflow workflow);
        WorkflowStartedEvent WorkflowStartedEvent();
        bool HasActiveEvent();
        IEnumerable<WorkflowItemEvent> AllActivityEventsFor(ActivityItem activityItem);
        IEnumerable<WorkflowItemEvent> AllTimerEventsFor(TimerItem timerItem);
        IEnumerable<MarkerRecordedEvent> AllMarkerRecordedEvents();
        IEnumerable<WorkflowSignaledEvent> AllSignalEvents();
        IEnumerable<WorkflowCancellationRequestedEvent> AllWorkflowCancellationRequestedEvents();
    }
}