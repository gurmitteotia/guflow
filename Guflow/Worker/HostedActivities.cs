using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Properties;

namespace Guflow.Worker
{
    public class HostedActivities : IDisposable, IHostedItems
    {
        private readonly Domain _domain;
        private readonly Activities _activities;
        private readonly CancellationTokenSource _cancellationTokenSource  = new CancellationTokenSource();
        private ErrorHandler _genericErrorHandler = ErrorHandler.NotHandled;
        private ErrorHandler _pollingErrorHandler = ErrorHandler.NotHandled;
        private ErrorHandler _responseErrorHandler = ErrorHandler.NotHandled;
        private ActivityExecution _activityExecution;
        private volatile bool _stopped;
        private volatile bool _disposed;
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
            _activities = new Activities(activitiesTypes, instanceCreator);
            _activityExecution = ActivityExecution.Concurrent((uint)Environment.ProcessorCount);
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
        public void StartExecution()
        {
            if (_activities.Count != 1)
                throw new InvalidOperationException(Resources.Can_not_determine_the_task_list_to_poll_for_activity_task);

            var singleActivityType = _activities.Single();
            var activityDescription = ActivityDescriptionAttribute.FindOn(singleActivityType);
            if(string.IsNullOrEmpty(activityDescription.DefaultTaskListName))
                throw new InvalidOperationException(Resources.Default_task_list_is_missing);

            StartExecution(new TaskQueue(activityDescription.DefaultTaskListName));
        }

        public void StartExecution(TaskQueue taskQueue)
        {
            Ensure.NotNull(taskQueue, "taskQueue");
            if(_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if(_stopped)
                throw new InvalidOperationException(Resources.Activity_execution_already_stopped);

            ExecuteHostedActivitiesAsync(taskQueue);
        }
        public void StopExecution()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!_stopped)
            {
                _stopped = true;
                _cancellationTokenSource.Cancel();
            }
        }
        public void OnPollingError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            OnPollingError(errorHandler.OnError);
        }
        public void OnPollingError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _pollingErrorHandler = ErrorHandler.Default(handleError).WithFallback(_genericErrorHandler);
        }
        public void OnResponseError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _responseErrorHandler = ErrorHandler.Default(handleError).WithFallback(_genericErrorHandler);
        }
        public void OnResponseError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            OnResponseError(errorHandler.OnError);
        }
        public void OnError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            OnError(errorHandler.OnError);
        }
        public void OnError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _genericErrorHandler = ErrorHandler.Default(handleError);
            _pollingErrorHandler = _pollingErrorHandler.WithFallback(_genericErrorHandler);
            _responseErrorHandler = _genericErrorHandler.WithFallback(_genericErrorHandler);
        }
        private async void ExecuteHostedActivitiesAsync(TaskQueue taskQueue)
        {
            var activityExecution = Execution;
            activityExecution.Set(hostedActivities: this);
            var domain = _domain.OnPollingError(_pollingErrorHandler);
            while (!_stopped)
            {
                var workerTask = await taskQueue.PollForWorkerTaskAsync(domain, _cancellationTokenSource.Token);
                workerTask.SetErrorHandler(_genericErrorHandler);
                await activityExecution.ExecuteAsync(workerTask);
            }
        }
        internal Activity FindBy(string activityName, string activityVersion)
        {
            return _activities.FindBy(activityName, activityVersion);
        }

        internal async Task SendAsync(ActivityResponse response)
        {
            var retryableFunc = new RetryableFunc(_responseErrorHandler);
            await retryableFunc.ExecuteAsync(()=>response.SendAsync(_domain.Client, _cancellationTokenSource.Token));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                StopExecution();
                _cancellationTokenSource.Dispose();
                _disposed = true;
            }
        }
        
    }
}