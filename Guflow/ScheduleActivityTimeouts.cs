using System;

namespace Guflow
{
    public struct ScheduleActivityTimeouts
    {
        public TimeSpan? HeartbeatTimeout { get; set; }
        public TimeSpan? ScheduleToCloseTimeout { get; set; }
        public TimeSpan? ScheduleToStartTimeout { get; set; }
        public TimeSpan? StartToCloseTimeout { get; set; }
    }
}