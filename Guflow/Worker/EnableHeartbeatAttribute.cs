// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Worker
{
    /// <summary>
    /// Enable the heartbeat on activity. Activity will start sending heartbeats to Amazon SWF when heartbeat is enabled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EnableHeartbeatAttribute : Attribute
    {
        /// <summary>
        /// Override default heartbeat interval. It is useful to keep heartbeat interval less than default(configured) heartbeat interval.
        /// </summary>
        public ulong IntervalInMilliSeconds { get; set; }
    }
}