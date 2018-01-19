// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow.Worker
{
    /// <summary>
    /// Represents activity hearbeat.
    /// </summary>
    public class ActivityHeartbeat
    {
        private Func<string> _activityDetailsFunc;
        private TimeSpan _interval;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _stopped;
        private ErrorHandler _errorHandler;
        private IErrorHandler _fallbackErrorHandler = ErrorHandler.NotHandled;
        private bool _enabled = false;
        internal ActivityHeartbeat()
        {
            _activityDetailsFunc = ()=>string.Empty;
            _cancellationTokenSource = new CancellationTokenSource();
            _errorHandler = ErrorHandler.NotHandled;
        }
        /// <summary>
        /// Raise this event cancellation is requested for activity.
        /// </summary>
        public event EventHandler CancellationRequested;
        /// <summary>
        /// Raised when activity is terminated by Amazon SWF. It can happen when activity is exceed its allowed execution time and timedout
        /// or workflow is terminated.
        /// </summary>
        public event EventHandler ActivityTerminated;
        /// <summary>
        /// Enable the activity heartbeat with given interval.
        /// </summary>
        /// <param name="interval"></param>
        public void Enable(TimeSpan interval)
        {
            if(interval <= TimeSpan.Zero)
                throw new ArgumentException(Resources.Invalid_heartbeat_interval, "interval");
            _interval = interval;
            _enabled = true;
        }
        /// <summary>
        /// Provide a func callback to provide the details for heartbeat pulse. Details from func callback will be send
        /// to Amazon SWF when reporting the heartbeat.
        /// </summary>
        /// <param name="details"></param>
        public void ProvideDetails(Func<string> details)
        {
            Ensure.NotNull(details, "getDetailsFunc");
            _activityDetailsFunc = details;
        }
        /// <summary>
        /// Register an error handler for any heartbeat specific error. If you do do not handle it here then ActivityHost generic error handler is used.
        /// </summary>
        /// <param name="handleError"></param>
        public void OnError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _errorHandler = ErrorHandler.Default(handleError).WithFallback(_fallbackErrorHandler);
        }

        internal void StartHeartbeatIfEnabled(IHeartbeatSwfApi heartbeatSwfApi, string taskToken)
        {
            if(!_enabled)
                return;
            StartHeartbeat(heartbeatSwfApi, taskToken);
        }

        private async void StartHeartbeat(IHeartbeatSwfApi heartbeatSwfApi, string taskToken)
        {
            var interval = _interval;
            while (!_stopped)
            {
               var waited = await WaitFor(interval);
                if (!waited)
                        continue;
               await ExecuteInRetryLoop(async () => await RecordHeartbeat(heartbeatSwfApi, taskToken));
            }
        }
        private async Task ExecuteInRetryLoop(Func<Task> action)
        {
            var error = new Error();
            var errorHandler = _errorHandler;
            bool retry = false;
            int retryAttempts = 0;
            do
            {
                try
                {
                    retry = false;
                    await action();
                }
                catch (UnknownResourceException)
                {
                    RaiseActivityTerminatedEvent();
                    StopHeartbeat();
                }
                catch (OperationCanceledException)
                {
                    //it means we are stopping the heartbeat.
                }
                catch (Exception exception)
                {
                    var errorAction = errorHandler.OnError(error.Set(exception, retryAttempts));
                    if (errorAction == ErrorAction.Unhandled)
                        throw;
                    if (errorAction == ErrorAction.Retry)
                        retry = true;
                }
                retryAttempts++;
            } while (retry);
        }

        private async Task<bool> WaitFor(TimeSpan interval)
        {
            try
            {
                await Task.Delay(interval, _cancellationTokenSource.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                //it means we are stopping the heartbeat.
                return false;
            }
        }

        internal void StopHeartbeat()
        {
            if (!_stopped)
            {
                _stopped = true;
                _cancellationTokenSource.Cancel();
            }
        }
        internal void SetFallbackErrorHandler(IErrorHandler errorHandler)
        {
            _fallbackErrorHandler = errorHandler;
            _errorHandler = _errorHandler.WithFallback(errorHandler);
        }
        private async Task RecordHeartbeat(IHeartbeatSwfApi heartbeatSwfApi, string token)
        {
            var details = _activityDetailsFunc();
            var isCancellationRequested = await heartbeatSwfApi.RecordHearbeatAsync(token,details, _cancellationTokenSource.Token);
            if (isCancellationRequested)
                RaiseCancellationRequestedEvent();
        }

        private void RaiseCancellationRequestedEvent()
        {
            CancellationRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseActivityTerminatedEvent()
        {
            ActivityTerminated?.Invoke(this, EventArgs.Empty);
        }
    }
}