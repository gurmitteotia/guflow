// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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

        public async Task ExecuteAsync(Func<Task> func)
        {
            var error = new Error();
            int retryAttempts = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    await func();
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
        }
        public T Execute<T>(Func<T> func, T defaultValue)
        {
            var error = new Error();
            int retryAttempts = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    return func();
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