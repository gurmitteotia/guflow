using System;

namespace Guflow.Worker
{
    public class ActivityDescriptionMissingException : Exception
    {
        public ActivityDescriptionMissingException(string message): base(message)
        {
        }
    }
}