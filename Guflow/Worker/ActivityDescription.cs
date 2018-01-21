// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;

namespace Guflow.Worker
{
    /// <summary>
    /// It describe an activity. Its attributes are used while registering the activity with Amazon SWF or communicating with Amazon SWF.
    /// </summary>
    public class ActivityDescription
    {
       /// <summary>
        /// Create the instance with activity name and version.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        public ActivityDescription(string name, string version)
        {
            Ensure.NotNullAndEmpty(name,nameof(name));
            Ensure.NotNullAndEmpty(version,nameof(version));

            Name = name;
            Version = version;
        }
        /// <summary>
        /// Gets the name of activity.
        /// </summary>
        public string Name { get; }

        //Gets or sets the version of activity.
        public string Version { get; }

        /*
        /// <summary>
        /// Gets or sets the description of activity.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the default task list name for activity.
        /// </summary>
        public string DefaultTaskListName { get; set; }

        /// <summary>
        /// Gets or sets the default priority for activity.
        /// </summary>
        public int DefaultTaskPriority { get; set; }

        public TimeSpan? DefaultStartToCloseTimeout { get; set; }

        public TimeSpan? DefaultHeartbeatTimeout { get; set; }
       

        public TimeSpan? DefaultScheduleToCloseTimeout { get; set; }
       
        public TimeSpan? DefaultScheduleToStartTimeout { get; set; }

        */
        internal static ActivityDescriptionAttribute FindOn<TActivity>() where TActivity : Activity
        {
            return FindOn(typeof(TActivity));
        }

        internal static ActivityDescriptionAttribute FindOn(Type activityType)
        {
            Ensure.NotNull(activityType, "activityType");
            return null;
        }
        /*
        internal RegisterActivityTypeRequest RegisterRequest(string domainName)
        {
            return new RegisterActivityTypeRequest
            {
                Name = Name,
                Version = Version,
                Description = Description,
                Domain = domainName,
                DefaultTaskList = DefaultTaskListName.TaskList(),
                DefaultTaskStartToCloseTimeout = DefaultStartToCloseTimeout.Seconds(),
                DefaultTaskPriority = DefaultTaskPriority.ToString(),
                DefaultTaskHeartbeatTimeout = DefaultHeartbeatTimeout.Seconds(),
                DefaultTaskScheduleToCloseTimeout = DefaultScheduleToCloseTimeout.Seconds(),
                DefaultTaskScheduleToStartTimeout = DefaultScheduleToStartTimeout.Seconds()
            };
        }*/
    }
}