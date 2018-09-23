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
        /// <summary>
        /// Create complete response.
        /// </summary>
        /// <param name="result">Result, it is serialized to JSON if it is complex object.</param>
        public ActivityCompletedResponse(object result)
        {
            _result = result.ToAwsString();
        }

        internal override async Task SendAsync(string taskToken, IAmazonSimpleWorkflow simpleWorkflow, CancellationToken cancellationToken)
        {
            var request = new RespondActivityTaskCompletedRequest() {Result = _result, TaskToken = taskToken};
            await simpleWorkflow.RespondActivityTaskCompletedAsync(request, cancellationToken);
        }

        private bool Equals(ActivityCompletedResponse other)
        {
            return string.Equals(_result, other._result);
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
                return ((_result != null ? _result.GetHashCode() : 0) * 397);
            }
        }
    }
}