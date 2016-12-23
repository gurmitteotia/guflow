using System;

namespace Guflow.Worker
{
    public class ActivityAlreadyHostedException : Exception
    {
        public ActivityAlreadyHostedException(string message):base(message)
        {
        }
    }
}