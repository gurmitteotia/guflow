using System;

namespace Guflow.Worker
{
    public class ActivityNotHostedException : Exception
    {
        public ActivityNotHostedException(string message) : base(message)
        {
        }
    }
}