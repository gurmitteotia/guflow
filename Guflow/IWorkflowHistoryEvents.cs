namespace Guflow
{
    public interface IWorkflowHistoryEvents
    {
        ActivityEvent LatestActivityEventFor(ActivityItem wrkflowItem);
        TimerFiredEvent LatestTimerEventFor(TimerItem timerItem);
    }
}