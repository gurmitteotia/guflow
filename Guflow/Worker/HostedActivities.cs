// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Worker
{
    internal class HostedActivities : IHostedActivities
    {
        private readonly Dictionary<string, Type> _hostedActivities = new Dictionary<string, Type>();
        private readonly Func<Type, Activity> _instanceCreator;
        
        public HostedActivities(IEnumerable<Type> activitiesTypes, Func<Type, Activity> instanceCreator)
        {
            _instanceCreator = instanceCreator;
            activitiesTypes = activitiesTypes.Where(w => w != null).ToArray();
            if (!activitiesTypes.Any())
                throw new ArgumentException(Resources.No_activity_to_host, "activitiesTypes");
            PopulateHostedActivities(activitiesTypes);
        }

        public Activity FindBy(string activityName, string activityVersion)
        {
            Type hostedActivityType;
            var hostedActivityKey = activityName + activityVersion;
            if (!_hostedActivities.TryGetValue(hostedActivityKey, out hostedActivityType))
                throw new ActivityNotHostedException(string.Format(Resources.Activity_not_hosted, activityName, activityVersion));
            var activityInstance = _instanceCreator(hostedActivityType);
            if (activityInstance == null)
                throw new ActivityInstanceCreationException(string.Format(Resources.Activity_instance_creation_failed, activityName, activityVersion));
            if (!MatchDescription(activityInstance, activityName, activityVersion))
                throw new ActivityInstanceMismatchedException(string.Format(Resources.Activity_instance_mismatch, activityInstance.GetType().Name, activityName, activityVersion));
            return activityInstance;
        }

        private static bool MatchDescription(Activity activityInstance, string activityName, string activityVersion)
        {
            var activityDescription = ActivityDescription.FindOn(activityInstance.GetType());
            return activityDescription.Name.Equals(activityName) &&
                   activityDescription.Version.Equals(activityVersion);
        }

        private void PopulateHostedActivities(IEnumerable<Type> activitiesTypes)
        {
            foreach (var activityType in activitiesTypes)
            {
                var activityDescription = ActivityDescription.FindOn(activityType);
                var hostedActivityKey = activityDescription.Name + activityDescription.Version;
                if (_hostedActivities.ContainsKey(hostedActivityKey))
                    throw new ActivityAlreadyHostedException(string.Format(Resources.Activity_already_hosted, activityDescription.Name, activityDescription.Version));
                _hostedActivities.Add(hostedActivityKey, activityType);
            }
        }

        public IEnumerable<ActivityDescription> ActivityDescriptions
            => _hostedActivities.Values.Select(ActivityDescription.FindOn);
    }
}