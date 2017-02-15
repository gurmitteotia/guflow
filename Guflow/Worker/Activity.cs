
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
                    return new ActivityFailedResponse(activityArgs.TaskToken, exception.GetType().Name, exception.Message);
                throw;
            }
            finally
            {
                Hearbeat.StopHeartbeat();
            }
        }

        protected ActivityResponse Completed(string result)
        {
            return new ActivityCompletedResponse(_currentTaskToken, result);
        }
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