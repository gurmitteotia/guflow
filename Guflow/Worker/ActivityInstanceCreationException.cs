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
        protected ActivityInstanceCreationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}