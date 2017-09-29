namespace Guflow
{
    public interface IHost
    {
        void StartExecution();
        void StartExecution(TaskQueue taskQueue);
        void StopExecution();
        void OnPollingError(IErrorHandler errorHandler);
        void OnPollingError(HandleError handleError);
        void OnResponseError(HandleError handleError);
        void OnResponseError(IErrorHandler errorHandler);
        void OnError(IErrorHandler errorHandler);
        void OnError(HandleError handleError);
    }
}