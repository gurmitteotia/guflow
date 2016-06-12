namespace Guflow
{
    public interface IActivityItem : IWorkflowItem
    {
        WorkflowItemEvent LatestEvent { get; }
        string Version { get; }
        string PositionalName { get; }
    }
}