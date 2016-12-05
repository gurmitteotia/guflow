using System;

namespace Guflow
{
    internal class RetryableFunc
    {
        private readonly TaskQueue.OnError _errorHandler;

        public RetryableFunc(TaskQueue.OnError errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public TReturn Execute<TReturn>(Func<TReturn> func) where TReturn:new()
        {
            int retryAttempt = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    //var request = taskQueue.CreateRequest(_name, nextPageToken);
                    //var response = await _amazonSimpleWorkflow.PollForDecisionTaskAsync(request);
                    //return response.DecisionTask;
                    return func();
                }
                catch (Exception exception)
                {
                    var errorAction = _errorHandler(exception, retryAttempt);
                    if (errorAction == ErrorAction.Unhandled)
                        throw;
                    if (errorAction == ErrorAction.Retry)
                        retry = true;
                }
                retryAttempt++;
            } while (retry);
            return new TReturn();
        }     
    }
}