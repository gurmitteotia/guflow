using System.Collections.Generic;

namespace Guflow.Decider
{
    public interface ITimerItem : IWorkflowItem
    {
        string Name { get; }
        IEnumerable<WorkflowItemEvent> AllEvents { get; }
    }
}