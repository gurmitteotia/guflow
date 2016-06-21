using System.Collections.Generic;

namespace Guflow
{
    internal interface IWorkflowHistoryEvents
    {
        WorkflowItemEvent LatestActivityEventFor(WorkflowItem activityItem);
        WorkflowItemEvent LatestTimerEventFor(WorkflowItem timerItem);
        IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflow workflow);
        WorkflowStartedEvent WorkflowStartedEvent();
        bool IsActive();
    }
}