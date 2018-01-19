// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    /// <summary>
    /// Provide various timeouts when scheduling the activity. These timeouts will override the default timeouts.
    /// </summary>
    public struct ActivityTimeouts
    {
        /// <summary>
        /// Heartbeat timeout of activity. If Amazon SWF does not receive heartbeat pulse within this period then activity will be timedout.
        /// </summary>
        public TimeSpan? HeartbeatTimeout { get; set; }
        
        public TimeSpan? ScheduleToCloseTimeout { get; set; }
        public TimeSpan? ScheduleToStartTimeout { get; set; }
        public TimeSpan? StartToCloseTimeout { get; set; }
    }
}