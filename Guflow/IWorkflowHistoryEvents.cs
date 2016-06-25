using System.Collections.Generic;

namespace Guflow
{
    internal interface IWorkflowHistoryEvents
    {
        WorkflowItemEvent LastActivityEventFor(WorkflowItem activityItem);
        WorkflowItemEvent LastTimerEventFor(WorkflowItem timerItem);
        IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflow workflow);
        WorkflowStartedEvent WorkflowStartedEvent();
        bool IsActive();
        ActivityCompletedEvent LastCompletedEventFor(ActivityItem activityItem);
        ActivityFailedEvent LastFailedEventFor(ActivityItem activityItem);
        ActivityTimedoutEvent LastTimedoutEventFor(ActivityItem activityItem);
        ActivityCancelledEvent LastCancelledEventFor(ActivityItem activityItem);
        IEnumerable<WorkflowItemEvent> AllEventsFor(ActivityItem activityItem);
    }
}