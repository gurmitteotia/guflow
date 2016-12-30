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

        protected ActivityExecutionMethodException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}