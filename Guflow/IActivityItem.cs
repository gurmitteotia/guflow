using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public interface IActivityItem : IWorkflowItem
    {
        ActivityCompletedEvent LastCompletedEvent { get; }
        ActivityFailedEvent LastFailedEvent { get; }
        ActivityTimedoutEvent LastTimedoutEvent { get; }
        ActivityCancelledEvent LastCancelledEvent { get; }
        IEnumerable<WorkflowItemEvent> AllEvents { get; }
        string Version { get; }
        string PositionalName { get; }
    }
}