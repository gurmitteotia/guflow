
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    /// <summary>
    /// Represent an activity.
    /// </summary>
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
        /// <summary>
        /// Successfully complete the activity with given result.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected ActivityResponse Complete(string result)
        {
            return new ActivityCompleteResponse(_currentTaskToken, result);
        }
        /// <summary>
        /// Cancel the activity with given details.
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        protected ActivityResponse Cancel(string details)
        {
            return new ActivityCancelResponse(_currentTaskToken, details);
        }
        /// <summary>
        /// Fails the activity with given reason and details.
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        protected ActivityResponse Fail(string reason,string details)
        {
            return new ActivityFailResponse(_currentTaskToken, reason, details);
        }
        /// <summary>
        /// Do not send any response to Amazon SWF.It is useful when you do not have response to return Amazon SWF and possibly a human intervention is need to send the response
        /// back to Amazon SWF. Later on you can send the reponse by using ActivityResponse classes.
        /// </summary>
        protected ActivityResponse Defer => ActivityResponse.Defer;

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