using System;
using System.Configuration;
using System.Reflection;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Properties;

namespace Guflow.Worker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ActivityDescriptionAttribute : Attribute
    {
        private uint? _defaultStartToCloseTimeoutInSeconds;
        private uint? _defaultHeartbeatTimeoutInSeconds;
        private uint? _defaultScheduleToCloseTimeoutInSeconds;
        private uint? _defaultScheduleToStartTimeout;
        public ActivityDescriptionAttribute(string version)
        {
            Version = version;
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string DefaultTaskListName { get; set; }
        public int DefaultTaskPriority { get; set; }

        public uint DefaultStartToCloseTimeoutInSeconds
        {
            get { return _defaultStartToCloseTimeoutInSeconds ?? default(uint); }
            set { _defaultStartToCloseTimeoutInSeconds = value; }
        }

        public uint DefaultHeartbeatTimeoutInSeconds
        {
            get { return _defaultHeartbeatTimeoutInSeconds ?? default(uint); }
            set { _defaultHeartbeatTimeoutInSeconds = value; }
        }

        public uint DefaultScheduleToCloseTimeoutInSeconds
        {
            get { return _defaultScheduleToCloseTimeoutInSeconds ?? default(uint); }
            set { _defaultScheduleToCloseTimeoutInSeconds = value; }
        }

        public uint DefaultScheduleToStartTimeoutInSeconds
        {
            get { return _defaultScheduleToStartTimeout ?? default(uint); }
            set { _defaultScheduleToStartTimeout = value; }
        }

        internal string DefaultStartToCloseTimeout { get { return _defaultStartToCloseTimeoutInSeconds.SwfFormat(); } }
        internal string DefaultHeartbeatTimeout { get { return _defaultHeartbeatTimeoutInSeconds.SwfFormat(); } }
        internal string DefaultScheduleToCloseTimeout { get {return  _defaultScheduleToCloseTimeoutInSeconds.SwfFormat(); } }
        internal string DefaultScheduleToStartTimeout { get { return _defaultScheduleToStartTimeout.SwfFormat(); } }
        public static ActivityDescriptionAttribute FindOn<TActivity>() where TActivity : Activity
        {
            return FindOn(typeof(TActivity));
        }
        public static ActivityDescriptionAttribute FindOn(Type activityType)
        {
            Ensure.NotNull(activityType, "activityType");

            if (!typeof(Activity).IsAssignableFrom(activityType))
                throw new NonActivityTypeException(string.Format(Resources.Non_activity_type, activityType.Name, typeof(Activity).Name));

            var activityDescription = activityType.GetCustomAttribute<ActivityDescriptionAttribute>();
            if (activityDescription == null)
                throw new ActivityDescriptionMissingException(string.Format(Resources.Activity_attribute_missing, activityType.Name));

            if (string.IsNullOrWhiteSpace(activityDescription.Version))
                throw new ConfigurationErrorsException(string.Format(Resources.Empty_version, activityType.Name));

            if (string.IsNullOrWhiteSpace(activityDescription.Name))
                activityDescription.Name = activityType.Name;
            return activityDescription;
        }

        //internal RegisterActivityTypeRequest RegisterRequest(string domainName)
        //{
        //    return new RegisterActivityTypeRequest
        //    {
        //        Name = Name,
        //        Version = Version,
        //        Description = Description,
        //        Domain = domainName,
        //        DefaultTaskList = DefaultTaskListName.TaskList(),
        //        DefaultTaskStartToCloseTimeout = DefaultStartToCloseTimeout,
        //        DefaultTaskPriority = DefaultTaskPriority.ToString(),
        //        DefaultTaskHeartbeatTimeout = DefaultHeartbeatTimeout,
        //        DefaultTaskScheduleToCloseTimeout = DefaultScheduleToCloseTimeout,
        //        DefaultTaskScheduleToStartTimeout = DefaultScheduleToStartTimeout
        //    };
        //}
    }
}