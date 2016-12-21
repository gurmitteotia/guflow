namespace Guflow.Decider
{
    public interface IActivityTaskPoller
    {
        void PollForNewTask();

        void StopPolling();
    }
}