using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public abstract class ActivityResponse
    {
        public static readonly ActivityResponse Defferred = new DeferredResponse();

        public abstract Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow, CancellationToken cancellationToken);

        private class DeferredResponse : ActivityResponse
        {
            public override Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow, CancellationToken cancellationToken)
            {
                return Task.FromResult(false);
            }
        }
    }
}