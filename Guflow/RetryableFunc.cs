using System;
using System.Threading.Tasks;

namespace Guflow
{
    internal class RetryableFunc
    {
        private readonly IErrorHandler _errorHandler;
        public RetryableFunc(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, T defaultValue)
        {
            var error = new Error();
            int retryAttempts = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    return await func();
                }
                catch (Exception exception)
                {
                    var errorAction = _errorHandler.OnError(error.Set(exception, retryAttempts));
                    if (errorAction.IsRethrow)
                        throw;
                    if (errorAction.IsRetry)
                        retry = true;
                }
                retryAttempts++;
            } while (retry);
            return defaultValue;
        }
    }
}