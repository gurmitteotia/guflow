using System.Collections.Generic;

namespace Guflow.Decider
{
    public interface IActivityItem : IWorkflowItem
    {
        ActivityCompletedEvent LastCompletedEvent { get; }
        ActivityFailedEvent LastFailedEvent { get; }
        ActivityTimedoutEvent LastTimedoutEvent { get; }
        ActivityCancelledEvent LastCancelledEvent { get; }
        string Name { get; }
        string Version { get; }
        string PositionalName { get; }
    }
}