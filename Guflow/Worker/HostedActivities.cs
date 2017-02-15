using System;
using System.Collections.Generic;
using System.Threading;

namespace Guflow.Worker
{
    public class HostedActivities
    {
        private readonly Domain _domain;
        private readonly Activities _activities;
        private volatile bool _stopped;
        private CancellationTokenSource _cancellationTokenSource  = new CancellationTokenSource(); 
        internal HostedActivities(Domain domain, IEnumerable<Activity> activities)
        {
            Ensure.NotNull(domain, "domain");
            Ensure.NotNull(activities, "activities");
            _domain = domain;
            _activities = Activities.Singleton(activities);
        }

        internal HostedActivities(Domain domain, IEnumerable<Type> activitiesTypes)
            : this(domain, activitiesTypes, t=>(Activity)Activator.CreateInstance(t))
        {
        }

        public void StartExecution(TaskQueue taskQueue)
        {
            
        }

        private async void ExecuteHostedActivities(TaskQueue taskQueue)
        {
            while (_stopped)
            {
                var workerTask = await taskQueue.PollForWorkerTaskAsync(_domain);
                await workerTask.ExecuteFor(this);
            }
        }

        internal HostedActivities(Domain domain, IEnumerable<Type> activitiesTypes, Func<Type, Activity> instanceCreator)
        {
            Ensure.NotNull(domain, "domain");
            Ensure.NotNull(activitiesTypes, "activitiesTypes");
            Ensure.NotNull(instanceCreator, "instanceCreator");

            _domain = domain;
            _activities = Activities.Transient(activitiesTypes, instanceCreator);
        }

        internal Activity FindBy(string activityName, string activityVersion)
        {
            return _activities.FindBy(activityName, activityVersion);
        }
    }
}