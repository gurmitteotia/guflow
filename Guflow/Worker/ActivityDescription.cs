// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Reflection;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Properties;

namespace Guflow.Worker
{
    /// <summary>
    /// It describe an activity. Its attributes are used while registering the activity with Amazon SWF or communicating with Amazon SWF.
    /// </summary>
    public class ActivityDescription
    {
        private static readonly IDescriptionStrategy Strategy = new CompositeDescriptionStrategy(new []{DescriptionStrategy.FactoryMethod, DescriptionStrategy.FromAttribute});
       /// <summary>
        /// Create the instance with activity name and version.
        /// </summary>
        /// <param name="version"></param>
        public ActivityDescription(string version)
        {
            Ensure.NotNullAndEmpty(version,nameof(version), Resources.Empty_version);
            Version = version;
        }
        /// <summary>
        /// Gets or sets the name of activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of activity.
        /// </summary>
        public string Version { get; }
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
        /// <summary>
        /// Gets or sets the default star to close timeout for activity task.
        /// </summary>
        public TimeSpan? DefaultStartToCloseTimeout { get; set; }
        /// <summary>
        /// Gets or sets the default heartbeat timeout for activity task.
        /// </summary>
        public TimeSpan? DefaultHeartbeatTimeout { get; set; }
        /// <summary>
        /// Gets or sets default schedule to close timeout for activity task.
        /// </summary>
        public TimeSpan? DefaultScheduleToCloseTimeout { get; set; }
        /// <summary>
        /// Gets or sets the default schedule to start timeout for activity task.
        /// </summary>
        public TimeSpan? DefaultScheduleToStartTimeout { get; set; }
        
        internal static ActivityDescription FindOn<TActivity>() where TActivity : Activity
        {
            return FindOn(typeof(TActivity));
        }

        internal static ActivityDescription FindOn(Type activityType)
        {
            Ensure.NotNull(activityType, "activityType");
            if (!typeof(Activity).IsAssignableFrom(activityType))
                throw new NonActivityTypeException(string.Format(Resources.Non_activity_type, activityType.Name, typeof(Activity).Name));
            var activityDescription = Strategy.FindDescription(activityType);

            if (activityDescription == null)
                throw new ActivityDescriptionMissingException(string.Format(Resources.Activity_description_missing, activityType.Name));

            if (string.IsNullOrEmpty(activityDescription.Name)) activityDescription.Name = activityType.Name;
            return activityDescription;
        }
        
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
        }
    }
}