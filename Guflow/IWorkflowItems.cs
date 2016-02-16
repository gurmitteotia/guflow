using System.Collections.Generic;

namespace Guflow
{
    public interface IWorkflowItems
    {
        IEnumerable<WorkflowItem> GetStartupItems();

        IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem item);

        WorkflowItem Find(string name, string version, string positionalName);

        ActivityItem FindActivity(string name, string version, string positionalName);
    }
}