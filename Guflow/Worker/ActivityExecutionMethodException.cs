// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace Guflow.Worker
{
    [Serializable]
    public class ActivityExecutionMethodException : Exception
    {
        public ActivityExecutionMethodException()
        {
        }

        public ActivityExecutionMethodException(string message) : base(message)
        {
        }

        public ActivityExecutionMethodException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}