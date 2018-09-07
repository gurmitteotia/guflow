// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace Guflow.Worker
{
    [Serializable]
    public class ActivityInstanceCreationException : Exception
    {
        public ActivityInstanceCreationException()
        {
        }

        public ActivityInstanceCreationException(string message) : base(message)
        {
        }

        public ActivityInstanceCreationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}