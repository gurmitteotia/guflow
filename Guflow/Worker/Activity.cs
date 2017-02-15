
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

                Hearbeat.StartHeartbeatAsync(_amazonSimpleWorkflow, activityArgs.TaskToken);
                return await _executionMethod.Execute(this, activityArgs);
            }
            catch (Exception exception)
            {
                if (FailOnException)
                    return ActivityResponse.Fail(exception.GetType().Name, exception.Message);
                throw;
            }
            finally
            {
                Hearbeat.StopHeartbeat();
            }
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