
using System;
using System.Threading.Tasks;

namespace Guflow.Worker
{
    public abstract class Activity
    {
        private readonly ActivityExecutionMethod _executionMethod;
        protected bool FailOnException;
        protected Activity()
        {
           _executionMethod = new ActivityExecutionMethod(GetType());
            FailOnException = true;
        }

        internal async Task<ActivityResponse> ExecuteAsync(ActivityArgs activityArgs)
        {
            try
            {
                return await _executionMethod.Execute(this, activityArgs);
            }
            catch (Exception exception)
            {
                if(FailOnException)
                    return ActivityResponse.Fail(exception.GetType().Name, exception.Message);
                throw;
            }
        }
    }
}