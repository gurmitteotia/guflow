namespace Guflow
{
    public interface ITimerItem : IWorkflowItem
    {
        TimerEvent LatestEvent { get; }
    }
}