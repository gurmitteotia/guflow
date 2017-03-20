using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    public class ActivityCancelResponse : ActivityResponse
    {
        private readonly string _taskToken;
        private readonly string _details;

        public ActivityCancelResponse(string taskToken, string details)
        {
            Ensure.NotNullAndEmpty(taskToken, "taskToken");

            _taskToken = taskToken;
            _details = details;
        }

        public override async Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow, CancellationToken cancellationToken)
        {
            var request = new RespondActivityTaskCanceledRequest() {TaskToken = _taskToken, Details = _details};
            await simpleWorkflow.RespondActivityTaskCanceledAsync(request, cancellationToken);
        }

        private bool Equals(ActivityCancelResponse other)
        {
            return string.Equals(_taskToken, other._taskToken) && string.Equals(_details, other._details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ActivityCancelResponse)obj);
        }

        public override int GetHashCode()
        {
            return  _taskToken.GetHashCode() ^ (_details != null ? _details.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("ActivityCancelResponse: token {0}, details {1}", _taskToken, _details);
        }
    }
}