using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Worker
{
    public class HostedActivities
    {
        private readonly Domain _domain;
        private readonly Dictionary<string,Activity> _hostedActivities = new Dictionary<string, Activity>();
        internal HostedActivities(Domain domain, IEnumerable<Activity> activities)
        {
            Ensure.NotNull(_domain, "domain");
            Ensure.NotNull(activities, "activities");
            activities = activities.Where(w => w != null).ToArray();
            if (!activities.Any())
                throw new ArgumentException(Resources.No_activity_to_host, "activities");
            _domain = domain;
            PopulateHostedWorkflows(activities);
        }

        public Activity FindBy(string activityName, string activityVersion)
        {
            Activity hostedActivity;
            var hostedActivityKey = activityName + activityVersion;
            if (!_hostedActivities.TryGetValue(hostedActivityKey, out hostedActivity))
                throw new ActivityNotHostedException(string.Format(Resources.Activity_not_hosted, activityName, activityVersion));
            return hostedActivity;
        }

        private void PopulateHostedWorkflows(IEnumerable<Activity> activities)
        {
            foreach (var activity in activities)
            {
                var activityDescription = ActivityDescriptionAttribute.FindOn(activity.GetType());
                var hostedActivityKey = activityDescription.Name + activityDescription.Version;
                if (_hostedActivities.ContainsKey(hostedActivityKey))
                    throw new ActivityAlreadyHostedException(string.Format(Resources.Activity_already_hosted, activityDescription.Name, activityDescription.Version));
                _hostedActivities.Add(hostedActivityKey, activity);
            }
        }
    }
}