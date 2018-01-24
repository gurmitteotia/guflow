// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Worker
{
    /// <summary>
    /// Describe the activities. Its properties are used when registering the activity with Amazon SWF.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ActivityDescriptionAttribute : Attribute
    {
       
        public ActivityDescriptionAttribute(string version)
        {
            Version = version;
        }

        /// <summary>
        /// Gets or sets the name of activity.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the version of activity.
        /// </summary>
        public string Version { get; }
        /// <summary>
        /// Gets or sets the description of activity.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the default task list name.
        /// </summary>
        public string DefaultTaskListName { get; set; }
        /// <summary>
        /// Gets or sets the default task priority.
        /// </summary>
        public int DefaultTaskPriority { get; set; }

        /// <summary>
        /// Gets or sets the activity task start to close timeout in seconds.
        /// </summary>
        public uint DefaultStartToCloseTimeoutInSeconds { get; set; }
      
        /// <summary>
        /// Gets or sets the activity task heartbeat timeout in seconds.
        /// </summary>
        public uint DefaultHeartbeatTimeoutInSeconds { get; set; }
   
        /// <summary>
        /// Gets or sets the activity schedule to close timeout in seconds.
        /// </summary>
        public uint DefaultScheduleToCloseTimeoutInSeconds { get; set; }
      
        /// <summary>
        /// Gets or sets the activity schedule to start timeout in seconds.
        /// </summary>
        public uint DefaultScheduleToStartTimeoutInSeconds { get; set; }
       
     
        internal ActivityDescription ActivityDescription()
        {
            return new ActivityDescription(Version)
            {
                Name = Name,
                Description = Description,
                DefaultTaskListName = DefaultTaskListName,
                DefaultTaskPriority = DefaultTaskPriority,
                DefaultHeartbeatTimeout = AwsFormat(DefaultHeartbeatTimeoutInSeconds),
                DefaultScheduleToCloseTimeout = AwsFormat(DefaultScheduleToCloseTimeoutInSeconds),
                DefaultScheduleToStartTimeout = AwsFormat(DefaultScheduleToStartTimeoutInSeconds),
                DefaultStartToCloseTimeout = AwsFormat(DefaultStartToCloseTimeoutInSeconds)
            };
        }

        private TimeSpan? AwsFormat(uint seconds)
        {
            if (seconds == 0) return null;
            return TimeSpan.FromSeconds(seconds);
        }
    }
}