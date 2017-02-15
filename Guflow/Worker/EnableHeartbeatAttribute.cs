using System;

namespace Guflow.Worker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EnableHeartbeatAttribute : Attribute
    {
        public EnableHeartbeatAttribute(ulong heartbeatIntervalInMilliSeconds)
        {
            HeartbeatIntervalInMilliSeconds = heartbeatIntervalInMilliSeconds;
        }
        public ulong HeartbeatIntervalInMilliSeconds { get; private set; }
    }
}