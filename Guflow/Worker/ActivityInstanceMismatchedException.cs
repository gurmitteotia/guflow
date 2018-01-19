// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace Guflow.Worker
{
    [Serializable]
    public class ActivityInstanceMismatchedException : Exception
    {
        public ActivityInstanceMismatchedException()
        {
        }

        public ActivityInstanceMismatchedException(string message) : base(message)
        {
        }

        public ActivityInstanceMismatchedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ActivityInstanceMismatchedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}