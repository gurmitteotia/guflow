using System.Collections.Generic;

namespace Guflow
{
    internal interface IWorkflowHistoryEvents
    {
        WorkflowItemEvent LatestEventFor(ActivityItem wrkflowItem);
        TimerFiredEvent LatestEventFor(TimerItem timerItem);
        IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflow workflow);
        WorkflowStartedEvent WorkflowStartedEvent();
        bool IsActive();
    }
}