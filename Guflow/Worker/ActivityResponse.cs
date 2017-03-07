using System.Threading.Tasks;
using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public abstract class ActivityResponse
    {
        public static readonly ActivityResponse Defferred = new DeferredResponse();

        public abstract Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow);

        private class DeferredResponse : ActivityResponse
        {
            public override Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow)
            {
                return Task.FromResult(false);
            }
        }
    }
}