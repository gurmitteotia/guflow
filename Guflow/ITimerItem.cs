namespace Guflow
{
    public interface ITimerItem : IWorkflowItem
    {
        WorkflowItemEvent LatestEvent { get; }
    }
}