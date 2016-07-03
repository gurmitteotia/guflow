namespace Guflow
{
    public interface IActivityItem : IWorkflowItem
    {
        ActivityCompletedEvent LastCompletedEvent { get; }
        ActivityFailedEvent LastFailedEvent { get; }
        ActivityTimedoutEvent LastTimedoutEvent { get; }
        ActivityCancelledEvent LastCancelledEvent { get; }
        string Version { get; }
        string PositionalName { get; }
    }
}