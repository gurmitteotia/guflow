using System.Collections.Generic;

namespace Guflow
{
    internal interface IWorkflowEvents
    {
        WorkflowItemEvent LastActivityEventFor(ActivityItem activityItem);
        WorkflowItemEvent LastTimerEventFor(TimerItem timerItem);
        IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflowActions workflow);
        WorkflowStartedEvent WorkflowStartedEvent();
        bool IsActive();
        ActivityCompletedEvent LastCompletedEventFor(ActivityItem activityItem);
        ActivityFailedEvent LastFailedEventFor(ActivityItem activityItem);
        ActivityTimedoutEvent LastTimedoutEventFor(ActivityItem activityItem);
        ActivityCancelledEvent LastCancelledEventFor(ActivityItem activityItem);
        IEnumerable<WorkflowItemEvent> AllActivityEventsFor(ActivityItem activityItem);
        IEnumerable<WorkflowItemEvent> AllTimerEventsFor(TimerItem timerItem);
        IEnumerable<MarkerRecordedEvent> AllMarkerRecordedEvents();
        IEnumerable<WorkflowSignaledEvent> AllSignalEvents();
        IEnumerable<WorkflowCancellationRequestedEvent> AllWorkflowCancellationRequestedEvents();
    }
}