using System;

namespace Guflow
{
    public class Error
    {
        public Exception Exception { get; private set; }

        public int RetryAttempts { get; private set; }

        internal Error Set(Exception exception, int retryAttempts)
        {
            Exception = exception;
            RetryAttempts = retryAttempts;
            return this;
        }
    }
}