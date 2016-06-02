using System.Collections.Generic;

namespace Guflow
{
    internal interface IWorkflowHistoryEvents
    {
        ActivityEvent LatestActivityEventFor(ActivityItem wrkflowItem);
        TimerFiredEvent LatestTimerEventFor(TimerItem timerItem);
        IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflow workflow);
        WorkflowStartedEvent WorkflowStartedEvent();
    }
}