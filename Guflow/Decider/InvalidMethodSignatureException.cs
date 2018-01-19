// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    public class InvalidMethodSignatureException : Exception
    {
        public InvalidMethodSignatureException(string message):base(message)
        {
        }

        public InvalidMethodSignatureException(string message, Exception innerException)
            :base(message, innerException)
        {
        }
    }
}