using System.Collections.Generic;

namespace Guflow
{
    public interface IWorkflowItem
    {
        IEnumerable<IActivityItem> ParentActivities { get; }

        IEnumerable<ITimerItem> ParentTimers{ get; }

        string Name { get; }
    }
}