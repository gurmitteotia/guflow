using System;

namespace Guflow.Worker
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EnableHeartbeatAttribute : Attribute
    {
        public ulong IntervalInMilliSeconds { get; set; }
    }
}