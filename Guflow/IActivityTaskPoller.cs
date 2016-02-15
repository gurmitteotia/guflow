namespace Guflow
{
    public interface IActivityTaskPoller
    {
        void PollForNewTask();

        void StopPolling();
    }
}