namespace Guflow.Decider
{
    public interface IActivityItem : IWorkflowItem
    {
        string Name { get; }
        string Version { get; }
        string PositionalName { get; }
    }
}