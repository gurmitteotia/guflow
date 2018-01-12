using System.Collections.Generic;

namespace Guflow.Decider
{
    internal interface IWorkflowHistoryEvents
    {
        WorkflowItemEvent LastActivityEventFor(ActivityItem activityItem);
        WorkflowItemEvent LastTimerEventFor(TimerItem timerItem);
        IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflow workflow);
        WorkflowStartedEvent WorkflowStartedEvent();
        bool HasActiveEvent();
        IEnumerable<WorkflowEvent> NewEvents();
        IEnumerable<WorkflowItemEvent> AllActivityEventsFor(ActivityItem activityItem);
        IEnumerable<WorkflowItemEvent> AllTimerEventsFor(TimerItem timerItem);
        IEnumerable<MarkerRecordedEvent> AllMarkerRecordedEvents();
        IEnumerable<WorkflowSignaledEvent> AllSignalEvents();
        IEnumerable<WorkflowCancellationRequestedEvent> AllWorkflowCancellationRequestedEvents();
    }
}