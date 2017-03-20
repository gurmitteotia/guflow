
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public abstract class Activity
    {
        private readonly ActivityExecutionMethod _executionMethod;
        private IAmazonSimpleWorkflow _amazonSimpleWorkflow;
        private IErrorHandler _errorHandler = ErrorHandler.NotHandled;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected bool FailOnException;
        protected readonly ActivityHeartbeat Hearbeat = new ActivityHeartbeat();
        private string _currentTaskToken = string.Empty;
        protected Activity()
        {
            _executionMethod = new ActivityExecutionMethod(GetType());
            ConfigureHeartbeat();
            FailOnException = true;
        }

        internal void SetAmazonSwfClient(IAmazonSimpleWorkflow amazonSwf)
        {
            _amazonSimpleWorkflow = amazonSwf;
        }
        internal void SetErrorHandler(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        internal async Task<ActivityResponse> ExecuteAsync(ActivityArgs activityArgs)
        {
            try
            {
                _currentTaskToken = activityArgs.TaskToken;
                Hearbeat.SetFallbackErrorHandler(_errorHandler);
                Hearbeat.StartHeartbeatIfEnabled(_amazonSimpleWorkflow, activityArgs.TaskToken);
                var retryableFunc = new RetryableFunc(_errorHandler);
                return await retryableFunc.ExecuteAsync(()=>_executionMethod.ExecuteAsync(this, activityArgs, _cancellationTokenSource.Token), Defer);
            }
            catch (OperationCanceledException exception)
            {
                return Cancel(exception.Message);
            }
            catch (Exception exception)
            {
                if (FailOnException)
                    return Fail(exception.GetType().Name, exception.Message);
                throw;
            }
            finally
            {
                Hearbeat.StopHeartbeat();
            }
        }

        protected ActivityResponse Complete(string result)
        {
            return new ActivityCompleteResponse(_currentTaskToken, result);
        }

        protected ActivityResponse Cancel(string details)
        {
            return new ActivityCancelResponse(_currentTaskToken, details);
        }
        protected ActivityResponse Fail(string reason,string details)
        {
            return new ActivityFailResponse(_currentTaskToken, reason, details);
        }
        protected ActivityResponse Defer { get { return ActivityResponse.Defer;} }

        private void ConfigureHeartbeat()
        {
            var enableHearbeatAttribute = GetType().GetCustomAttribute<EnableHeartbeatAttribute>();
            if (enableHearbeatAttribute != null)
            {
                Hearbeat.SetInterval(TimeSpan.FromMilliseconds(enableHearbeatAttribute.HeartbeatIntervalInMilliSeconds));
            }
            Hearbeat.CancellationRequested += (s, e) => _cancellationTokenSource.Cancel();
        }
    }
}