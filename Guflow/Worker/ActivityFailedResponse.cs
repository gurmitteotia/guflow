// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    /// <summary>
    /// Represent a failed activity response.
    /// </summary>
    public sealed class ActivityFailedResponse : ActivityResponse
    {
        private readonly string _details;

        public ActivityFailedResponse(string reason, object details)
        {
            Reason = reason;
            _details = details.ToAwsString();
            Details = details;
        }
        /// <summary>
        /// Failed reason.
        /// </summary>
        public readonly string Reason;

        /// <summary>
        /// Failed details.
        /// </summary>
        public readonly object Details;

        internal override async Task SendAsync(string taskToken, IAmazonSimpleWorkflow simpleWorkflow,
            CancellationToken cancellationToken)
        {
            var request = new RespondActivityTaskFailedRequest()
            {
                TaskToken = taskToken,
                Reason = Reason,
                Details = _details
            };

            await simpleWorkflow.RespondActivityTaskFailedAsync(request, cancellationToken);
        }

        private bool Equals(ActivityFailedResponse other)
        {
            return string.Equals(Reason, other.Reason) && string.Equals(_details, other._details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ActivityFailedResponse && Equals((ActivityFailedResponse)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Reason != null ? Reason.GetHashCode() : 0) * 397) ^
                       (_details != null ? _details.GetHashCode() : 0);
            }
        }

    }
}