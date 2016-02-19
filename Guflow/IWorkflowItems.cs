using System.Collections.Generic;

namespace Guflow
{
    public interface IWorkflowItems
    {
        IEnumerable<WorkflowItem> GetStartupWorkflowItems();

        IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem item);

        WorkflowItem Find(string name, string version, string positionalName);

        WorkflowItem Find(Identity identity);

        ActivityItem FindActivity(string name, string version, string positionalName);

        ActivityItem FindActivity(Identity identity);

        TimerItem FindTimer(Identity identity);
    }
}