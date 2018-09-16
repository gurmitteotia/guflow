// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Properties;

namespace Guflow.Worker
{
    /// <summary>
    /// A host to execute the activities.
    /// </summary>
    public class ActivityHost : IDisposable, IHost
    {
        private readonly Domain _domain;
        private readonly Activities _activities;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ErrorHandler _genericErrorHandler = ErrorHandler.Continue;
        private ErrorHandler _pollingErrorHandler = ErrorHandler.NotHandled;
        private ErrorHandler _responseErrorHandler = ErrorHandler.NotHandled;
        private ActivityExecution _activityExecution;
        private volatile bool _disposed;
        private readonly HostState _state = new HostState();
        private readonly ILog _log = Log.GetLogger<ActivityHost>();
        private readonly ManualResetEventSlim _stoppedEvent = new ManualResetEventSlim(false);

        /// <summary>
        ///  Create activity host for domain and activities types. Activities should implement default constructor.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="activitiesTypes"></param>
        public ActivityHost(Domain domain, IEnumerable<Type> activitiesTypes)
            : this(domain, activitiesTypes, t => (Activity)Activator.CreateInstance(t))
        {
        }
        /// <summary>
        /// Create new instance of ActivityHost and let you control the creation of activity's instance.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="activitiesTypes"></param>
        /// <param name="instanceCreator"></param>
        public ActivityHost(Domain domain, IEnumerable<Type> activitiesTypes, Func<Type, Activity> instanceCreator)
        {
            Ensure.NotNull(domain, "domain");
            Ensure.NotNull(activitiesTypes, "activitiesTypes");
            Ensure.NotNull(instanceCreator, "instanceCreator");
            OnPollingError(e => ErrorAction.Unhandled);
            OnResponseError(e => ErrorAction.Unhandled);
            _domain = domain;
            PollingIdentity = Environment.GetEnvironmentVariable("COMPUTERNAME") ?? Environment.GetEnvironmentVariable("HOSTNAME");
            _activities = new Activities(activitiesTypes, instanceCreator);
            _activityExecution = ActivityExecution.Concurrent((uint)Environment.ProcessorCount);
        }
        /// <summary>
        /// Return the execution state.
        /// </summary>
        public HostStatus Status => _state.Status;
        /// <summary>
        /// Raised when an exception escaped all handlers. In faulted state it no more polls for new work.
        /// </summary>
        public event EventHandler<HostFaultEventArgs> OnFault;
        /// <summary>
        /// Gets or sets execution strategy to control the parallel execution of activities. 
        /// </summary>
        public ActivityExecution Execution
        {
            get => _activityExecution;
            set
            {
                Ensure.NotNull(value, "Execution");
                _activityExecution = value;
            }
        }
        /// <summary>
        /// Start execution of hosted activities. It will start polling for new activity task on default task list. This method is only useful when one activity is hosted.
        /// </summary>
        public void StartExecution()
        {
            var defaultTaskListName = DetectTaskList();
            StartExecution(new TaskList(defaultTaskListName));
        }

        /// <summary>
        /// Started execution of hosted activities on given task list. 
        /// </summary>
        /// <param name="taskList"></param>
        public void StartExecution(TaskList taskList)
        {
            Ensure.NotNull(taskList, "taskList");
            if (_disposed)
                throw new ObjectDisposedException(Resources.Activity_execution_already_stopped);
            var pollingIdentity = PollingIdentity;
            ExecuteHostedActivitiesAsync(taskList, pollingIdentity);
        }
        /// <summary>
        /// Shut down host.
        /// </summary>
        public void StopExecution()
        {
            if (!_disposed)
            {
                _state.Stop();
                _disposed = true;
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
            StopExecution();
        }
        /// <summary>
        /// Handle polling error on host. Any unhandled polling error is handled by generic <see cref="OnError"/> handler.
        /// </summary>
        /// <param name="handleError"></param>
        public void OnPollingError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _pollingErrorHandler = ErrorHandler.Default(handleError).WithFallback(_genericErrorHandler);
        }
        /// <summary>
        /// Handle response on host. Any unhandled repsonse error is handled by generic <see cref="OnError"/> handler.
        /// </summary>
        /// <param name="handleError"></param>
        public void OnResponseError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _responseErrorHandler = ErrorHandler.Default(handleError).WithFallback(_genericErrorHandler);
        }
        /// <summary>
        /// Top level generic error handler. Any exception unhandled at this stage will cause the host to be faulted.
        /// </summary>
        /// <param name="handleError"></param>
        public void OnError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _genericErrorHandler = ErrorHandler.Default(handleError);
            _pollingErrorHandler = _pollingErrorHandler.WithFallback(_genericErrorHandler);
            _responseErrorHandler = _genericErrorHandler.WithFallback(_genericErrorHandler);
        }
        /// <summary>
        /// Gets or sets the polling identity. It will be recorded in Amazon SWf events.
        /// </summary>
        public string PollingIdentity { get; set; }

        private async void ExecuteHostedActivitiesAsync(TaskList taskList, string pollingIdentity)
        {
            _state.Start();
            var activityExecution = Execution;
            activityExecution.Set(activityHost: this);
            var domain = _domain.OnPollingError(_pollingErrorHandler);
            try
            {
                while (!_disposed)
                {
                    _log.Debug($"Polling for activity tasks on queue {taskList} in domain {domain}");
                    var workerTask = await taskList.PollForWorkerTaskAsync(domain, pollingIdentity, _cancellationTokenSource.Token).ConfigureAwait(false);
                    workerTask.SetErrorHandler(_genericErrorHandler);
                    await activityExecution.ExecuteAsync(workerTask);
                }
                _state.Stop();
            }
            catch (OperationCanceledException)
            {
                _log.Info("Shutting down the host");
                _state.Stop();
            }
            catch (Exception exception)
            {
                Fault(exception);
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
            await retryableFunc.ExecuteAsync(() => _domain.SendActivityResponseAsync(response, _cancellationTokenSource.Token));
        }
        internal void Fault(Exception exception)
        {
            _log.Fatal("Hosted activities is faulted.", exception);
            StopExecution();
            var faultHandler = OnFault;
            faultHandler?.Invoke(this, new HostFaultEventArgs(exception));
            _state.Fault();
        }

        private string DetectTaskList()
        {
            var taskLists = _activities.ActivityDescriptions.Select(d => d.DefaultTaskListName).ToArray();
            var defaultTaskList = taskLists.FirstOrDefault(f => !string.IsNullOrEmpty(f));
            if (string.IsNullOrEmpty(defaultTaskList))
                throw new InvalidOperationException(Resources.Can_not_determine_the_task_list_to_poll_for_activity_task);

            if (taskLists.Any(f => f != defaultTaskList))
                throw new InvalidOperationException(Resources.Can_not_determine_the_task_list_to_poll_for_activity_task);

            return defaultTaskList;
        }
    }
}