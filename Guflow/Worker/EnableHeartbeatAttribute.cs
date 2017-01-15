using System;

namespace Guflow.Worker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EnableHeartbeatAttribute : Attribute
    {
        public ulong HeartbeatIntervalInMilliSeconds { get; set; }
    }
}