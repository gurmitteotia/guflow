using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow.Worker
{
    public class ActivityHeartbeat
    {
        private Func<string> _activityDetailsFunc;
        private TimeSpan _interval;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _stopped;
        private IErrorHandler _errorHandler;
        private bool _enabled = false;
        public ActivityHeartbeat()
        {
            _activityDetailsFunc = ()=>string.Empty;
            _cancellationTokenSource = new CancellationTokenSource();
            _errorHandler = ErrorHandler.NotHandled;
        }
        public event EventHandler CancellationRequested;
        public event EventHandler ActivityTerminated;

        public void SetInterval(TimeSpan interval)
        {
            if(interval <= TimeSpan.Zero)
                throw new ArgumentException(Resources.Invalid_heartbeat_interval, "interval");
            _interval = interval;
            _enabled = true;
        }

        public void ProvideDetails(Func<string> getDetailsFunc)
        {
            Ensure.NotNull(getDetailsFunc, "getDetailsFunc");
            _activityDetailsFunc = getDetailsFunc;
        }
        public void OnError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            _errorHandler = errorHandler;
        }
        public void OnError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            OnError(ErrorHandler.Default(handleError));
        }

        internal async void StartHeartbeatAsync(IAmazonSimpleWorkflow amazonSimpleWorkflow, string taskToken)
        {
            if(!_enabled)
                return;
            var interval = _interval;
            while (! _stopped)
            {
                await WaitFor(interval);

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
                catch (TaskCanceledException)
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

        private async Task WaitFor(TimeSpan interval)
        {
            try
            {
                await Task.Delay(interval, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                //it means we are stopping the heartbeat.
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
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void RaiseActivityTerminatedEvent()
        {
            var handler = ActivityTerminated;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        private static bool IsCancellationRequested(RecordActivityTaskHeartbeatResponse response)
        {
            return response.ActivityTaskStatus.CancelRequested;
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