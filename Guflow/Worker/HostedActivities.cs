using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Guflow.Worker
{
    public class HostedActivities
    {
        private readonly Domain _domain;
        private readonly Activities _activities;
        private volatile bool _stopped;
        private CancellationTokenSource _cancellationTokenSource  = new CancellationTokenSource();
        private IErrorHandler _genericErrorHandler = ErrorHandler.NotHandled;
        private ActivityExecution _activityExecution;
        internal HostedActivities(Domain domain, IEnumerable<Activity> activities)
        {
            Ensure.NotNull(domain, "domain");
            Ensure.NotNull(activities, "activities");
            _domain = domain;
            _activities = Activities.Singleton(activities);
            Execution = ActivityExecution.Sequencial;
        }

        internal HostedActivities(Domain domain, IEnumerable<Type> activitiesTypes)
            : this(domain, activitiesTypes, t=>(Activity)Activator.CreateInstance(t))
        {
        }
        internal HostedActivities(Domain domain, IEnumerable<Type> activitiesTypes, Func<Type, Activity> instanceCreator)
        {
            Ensure.NotNull(domain, "domain");
            Ensure.NotNull(activitiesTypes, "activitiesTypes");
            Ensure.NotNull(instanceCreator, "instanceCreator");

            _domain = domain;
            _activities = Activities.Transient(activitiesTypes, instanceCreator);
        }

        public ActivityExecution Execution
        {
            get { return _activityExecution; }
            set
            {
                Ensure.NotNull(value, "Execution");
                _activityExecution = value;
            }
        }

        public void StartExecution(TaskQueue taskQueue)
        {
            Ensure.NotNull(taskQueue, "taskQueue");
            taskQueue = taskQueue.SetFallbackErrorHandler(_genericErrorHandler);
            ExecuteHostedActivitiesAsync(taskQueue);
        }

        public void OnError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            _genericErrorHandler = errorHandler;
        }
        public void OnError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            OnError(ErrorHandler.Default(handleError));
        }
        private async void ExecuteHostedActivitiesAsync(TaskQueue taskQueue)
        {
            var activityExecution = Execution;
            activityExecution.Set(hostedActivities: this);

            while (_stopped)
            {
                var workerTask = await taskQueue.PollForWorkerTaskAsync(_domain, _cancellationTokenSource.Token);
                await activityExecution.ExecuteAsync(workerTask);
            }
        }
        internal Activity FindBy(string activityName, string activityVersion)
        {
            return _activities.FindBy(activityName, activityVersion);
        }

        internal async Task SendAsync(ActivityResponse response)
        {
            await response.SendAsync(_domain.Client, _cancellationTokenSource.Token);
        }
    }
}