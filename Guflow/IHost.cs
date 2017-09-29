namespace Guflow
{
    public interface IHost
    {
        void StartExecution();
        void StartExecution(TaskQueue taskQueue);
        void StopExecution();
        void OnPollingError(HandleError handleError);
        void OnResponseError(HandleError handleError);
        void OnError(HandleError handleError);
    }
}