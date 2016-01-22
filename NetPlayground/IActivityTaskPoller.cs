namespace NetPlayground
{
    public interface IActivityTaskPoller
    {
        void PollForNewTask();

        void StopPolling();
    }
}