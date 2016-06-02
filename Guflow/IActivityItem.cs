namespace Guflow
{
    public interface IActivityItem : IWorkflowItem
    {
        ActivityEvent LatestEvent { get; }
        string Version { get; }
        string PositionalName { get; }
    }
}