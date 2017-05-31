using System.Collections.Generic;

namespace Guflow.Decider
{
    internal interface IWorkflowItems
    {
        IEnumerable<WorkflowItem> GetStartupWorkflowItems();

        IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem item);
        IEnumerable<WorkflowItem> GetParentsOf(WorkflowItem item);
        WorkflowItem Find(Identity identity);
        ActivityItem FindActivityFor(Identity identity);
        TimerItem FindTimerFor(Identity identity);
    }
}