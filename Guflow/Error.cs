// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow
{
    public class Error
    {
        /// <summary>
        /// Returns the exception being raised.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Returns number of times you have retried on errors. This counter will reset on success or ErrorAction.Continue
        /// </summary>
        public int RetryAttempts { get; private set; }

        internal Error Set(Exception exception, int retryAttempts)
        {
            Exception = exception;
            RetryAttempts = retryAttempts;
            return this;
        }
    }
}