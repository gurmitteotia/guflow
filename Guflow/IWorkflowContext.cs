namespace Guflow
{
    public interface IWorkflowContext
    {
        ActivityEvent LatestActivityEventFor(Identity identity);
        TimerFiredEvent LatestTimerEventFor(Identity identity);
    }
}