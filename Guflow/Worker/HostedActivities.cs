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
        private volatile bool _disposed;
        private readonly HostState _state = new HostState();
        private readonly ILog _log = Log.GetLogger<HostedActivities>();
        private readonly ManualResetEventSlim _stoppedEvent = new ManualResetEventSlim(false);
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

        public HostStatus Status => _state.Status;
        public event EventHandler<HostFaultEventArgs> OnFault; 
        public ActivityExecution Execution
        {
            get => _activityExecution;
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
                throw new ObjectDisposedException(Resources.Activity_execution_already_stopped);
            ExecuteHostedActivitiesAsync(taskQueue);
        }
        public void StopExecution()
        {
            if (_state.CanBeStopped())
            {
                _state.Stop();
                _cancellationTokenSource.Cancel();
                _stoppedEvent.Wait(TimeSpan.FromSeconds(5));
                _cancellationTokenSource.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                StopExecution();
                _disposed = true;
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
            _state.Start();
            var activityExecution = Execution;
            activityExecution.Set(hostedActivities: this);
            var domain = _domain.OnPollingError(_pollingErrorHandler);
            try
            {
                while (!_disposed)
                {
                    var workerTask = await taskQueue.PollForWorkerTaskAsync(domain, _cancellationTokenSource.Token);
                    workerTask.SetErrorHandler(_genericErrorHandler);
                    await activityExecution.ExecuteAsync(workerTask);
                }
                _state.Stop();
            }
            catch (OperationCanceledException e)
            {
                _log.Info("Shutting down the host");
                _state.Stop();
            }
            catch (Exception exception)
            {
                _state.Fault();
                _log.Fatal("Hosted activities is faulted.", exception);
                OnFault?.Invoke(this, new HostFaultEventArgs(exception));
            }
            finally
            {
                _stoppedEvent.Set();
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
        internal void Fault(Exception exception)
        {
            StopExecution();
            var faultHandler = OnFault;
            faultHandler?.Invoke(this, new HostFaultEventArgs(exception));
            _state.Fault();
        }
    }
}