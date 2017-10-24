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
        public ActivityHeartbeat()
        {
            _activityDetailsFunc = ()=>string.Empty;
            _cancellationTokenSource = new CancellationTokenSource();
            _errorHandler = ErrorHandler.NotHandled;
        }
        public event EventHandler CancellationRequested;
        public event EventHandler ActivityTerminated;
        /// <summary>
        /// Configure the heartbeat interval. It override the heartbeat interval given in ActivityDescriptionAttribute.
        /// </summary>
        /// <param name="interval"></param>
        public void SetInterval(TimeSpan interval)
        {
            if(interval <= TimeSpan.Zero)
                throw new ArgumentException(Resources.Invalid_heartbeat_interval, "interval");
            _interval = interval;
            _enabled = true;
        }
        /// <summary>
        /// Provides a function to send details to Amazon SWF on each heartbeat pulse.
        /// </summary>
        /// <param name="detailsFunc"></param>
        public void ProvideDetails(Func<string> detailsFunc)
        {
            Ensure.NotNull(detailsFunc, "getDetailsFunc");
            _activityDetailsFunc = detailsFunc;
        }
        /// <summary>
        /// Register an error handler for any heartbeat specific error. If you do do not handle it here then it ActivitiesHost generic error handler is used.
        /// </summary>
        /// <param name="handleError"></param>
        public void OnError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _errorHandler = ErrorHandler.Default(handleError).WithFallback(_fallbackErrorHandler);
        }

        internal void StartHeartbeatIfEnabled(IAmazonSimpleWorkflow amazonSimpleWorkflow, string taskToken)
        {
            if(!_enabled)
                return;
            StartHeartbeat(amazonSimpleWorkflow, taskToken);
        }

        private async void StartHeartbeat(IAmazonSimpleWorkflow amazonSimpleWorkflow, string taskToken)
        {
            var interval = _interval;
            while (!_stopped)
            {
               var waited = await WaitFor(interval);
                if (!waited)
                        continue;
               await ExecuteInRetryLoop(async () => await RecordHeartbeat(amazonSimpleWorkflow, taskToken));
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
        private async Task RecordHeartbeat(IAmazonSimpleWorkflow amazonSimpleWorkflow, string token)
        {
            var details = _activityDetailsFunc();
            var heartbeatRequest = HeartbeatRequest(details, token);
            var response = await amazonSimpleWorkflow.RecordActivityTaskHeartbeatAsync(heartbeatRequest, _cancellationTokenSource.Token);
            if (IsCancellationRequested(response))
                RaiseCancellationRequestedEvent();
        }

        private void RaiseCancellationRequestedEvent()
        {
            var handler = CancellationRequested;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseActivityTerminatedEvent()
        {
            var handler = ActivityTerminated;
            handler?.Invoke(this, EventArgs.Empty);
        }
        private static bool IsCancellationRequested(RecordActivityTaskHeartbeatResponse response)
        {
            return (response?.ActivityTaskStatus?.CancelRequested).HasValue;
        }

        private static RecordActivityTaskHeartbeatRequest HeartbeatRequest(string details, string token)
        {
            return new RecordActivityTaskHeartbeatRequest
            {
                TaskToken = token,
                Details = details
            };
        }

       
    }
}