// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Worker
{
    public class NonActivityTypeException : Exception
    {
        public NonActivityTypeException(string message) : base(message)
        {
            
        }
    }
}