using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public class ActivityCancelledResponse : ActivityResponse
    {
        public override Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}