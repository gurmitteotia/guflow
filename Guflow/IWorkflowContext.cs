namespace Guflow
{
    public interface IWorkflowContext
    {
        ActivityEvent LatestActivityEventFor(ActivityItem wrkflowItem);
        TimerFiredEvent LatestTimerEventFor(TimerItem timerItem);
    }
}