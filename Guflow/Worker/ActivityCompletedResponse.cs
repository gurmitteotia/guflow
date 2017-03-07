using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    public sealed class ActivityCompletedResponse : ActivityResponse
    {
       
        private readonly string _result;
        private readonly string _taskToken;
        public ActivityCompletedResponse(string taskToken, string result)
        {
            _result = result;
            _taskToken = taskToken;
        }
        public override async Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow)
        {
            var request = new RespondActivityTaskCompletedRequest() {Result = _result, TaskToken = _taskToken};
            await simpleWorkflow.RespondActivityTaskCompletedAsync(request);
        }
        private bool Equals(ActivityCompletedResponse other)
        {
            return string.Equals(_result, other._result) && string.Equals(_taskToken, other._taskToken);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ActivityCompletedResponse && Equals((ActivityCompletedResponse)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_result != null ? _result.GetHashCode() : 0) * 397) ^  _taskToken.GetHashCode();
            }
        }

      
    }
}