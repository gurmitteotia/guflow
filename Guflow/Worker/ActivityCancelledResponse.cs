// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    /// <summary>
    /// Represents activity cancelled response.
    /// </summary>
    public class ActivityCancelledResponse : ActivityResponse
    {
        private readonly string _details;

        public ActivityCancelledResponse(object details)
        {
            _details = details.ToAwsString();
            Details = details;
        }

        /// <summary>
        /// Cancellation details.
        /// </summary>
        public readonly object Details;

        internal override async Task SendAsync(string taskToken, IAmazonSimpleWorkflow simpleWorkflow,
            CancellationToken cancellationToken)
        {
            var request = new RespondActivityTaskCanceledRequest() {TaskToken = taskToken, Details = _details};
            await simpleWorkflow.RespondActivityTaskCanceledAsync(request, cancellationToken);
        }

        private bool Equals(ActivityCancelledResponse other)
        {
            return string.Equals(_details, other._details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ActivityCancelledResponse)obj);
        }

        public override int GetHashCode()
        {
            return _details != null ? _details.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return string.Format("ActivityCancelResponse: details {0}", _details);
        }
    }
}