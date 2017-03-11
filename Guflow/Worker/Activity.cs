
using System;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public abstract class Activity
    {
        private readonly ActivityExecutionMethod _executionMethod;
        private IAmazonSimpleWorkflow _amazonSimpleWorkflow;
        protected bool FailOnException;
        protected readonly ActivityHeartbeat Hearbeat = new ActivityHeartbeat();
        private string _currentTaskToken = string.Empty;
        protected Activity()
        {
            _executionMethod = new ActivityExecutionMethod(GetType());
            ConfigureHeartbeatInterval();
            FailOnException = true;
        }

        internal void SetAmazonSwfClient(IAmazonSimpleWorkflow amazonSwf)
        {
            _amazonSimpleWorkflow = amazonSwf;
        }

        internal async Task<ActivityResponse> ExecuteAsync(ActivityArgs activityArgs)
        {
            try
            {
                _currentTaskToken = activityArgs.TaskToken;
                Hearbeat.StartHeartbeatAsync(_amazonSimpleWorkflow, activityArgs.TaskToken);
                return await _executionMethod.Execute(this, activityArgs);
            }
            catch (Exception exception)
            {
                if (FailOnException)
                    return new ActivityFailResponse(activityArgs.TaskToken, exception.GetType().Name, exception.Message);
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

        private void ConfigureHeartbeatInterval()
        {
            var enableHearbeatAttribute = GetType().GetCustomAttribute<EnableHeartbeatAttribute>();
            if (enableHearbeatAttribute != null)
            {
                Hearbeat.SetInterval(TimeSpan.FromMilliseconds(enableHearbeatAttribute.HeartbeatIntervalInMilliSeconds));
            }
        }
    }
}