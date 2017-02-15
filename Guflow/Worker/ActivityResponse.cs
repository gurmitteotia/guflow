using System.Threading.Tasks;
using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public class ActivityResponse
    {
        public static readonly ActivityResponse Defferred = new ActivityResponse();

        public async Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow)
        {
            
        }

    }
}