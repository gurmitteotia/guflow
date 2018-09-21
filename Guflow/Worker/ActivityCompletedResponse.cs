// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    /// <summary>
    /// Represents activity completed response.
    /// </summary>
    public sealed class ActivityCompletedResponse : ActivityResponse
    {
        private readonly string _result;
        private readonly string _taskToken;
        /// <summary>
        /// Create complete response.
        /// </summary>
        /// <param name="taskToken">Task token for activity this response belongs to.</param>
        /// <param name="result">Result, it is serialized to JSON if it is complex object.</param>
        public ActivityCompletedResponse(string taskToken, object result)
        {
            Ensure.NotNullAndEmpty(taskToken, "taskToken");
            _result = result.ToAwsString();
            _taskToken = taskToken;
        }

        internal override async Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow, CancellationToken cancellationToken)
        {
            var request = new RespondActivityTaskCompletedRequest() {Result = _result, TaskToken = _taskToken};
            await simpleWorkflow.RespondActivityTaskCompletedAsync(request, cancellationToken);
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
                return ((_result != null ? _result.GetHashCode() : 0) * 397) ^ _taskToken.GetHashCode();
            }
        }
    }
}