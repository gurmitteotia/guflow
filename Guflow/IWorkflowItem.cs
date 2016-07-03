using System.Collections.Generic;

namespace Guflow
{
    public interface IWorkflowItem
    {
        IEnumerable<IActivityItem> ParentActivities { get; }
        IEnumerable<ITimerItem> ParentTimers{ get; }
        WorkflowItemEvent LastEvent { get; }
        IEnumerable<WorkflowItemEvent> AllEvents { get; }
        bool IsActive { get; }
        string Name { get; }
    }
}