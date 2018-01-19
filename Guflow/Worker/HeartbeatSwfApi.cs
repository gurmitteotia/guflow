// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    internal class HeartbeatSwfApi : IHeartbeatSwfApi
    {
        private readonly IAmazonSimpleWorkflow _client;

        public HeartbeatSwfApi(IAmazonSimpleWorkflow client)
        {
            _client = client;
        }

        public async Task<bool> RecordHearbeatAsync(string token, string details, CancellationToken cancellationToken)
        {
            var request = HeartbeatRequest(details, token);
            var response = await _client.RecordActivityTaskHeartbeatAsync(request, cancellationToken);
            return IsCancellationRequested(response);
        }
        private static bool IsCancellationRequested(RecordActivityTaskHeartbeatResponse response)
        {
            return (response?.ActivityTaskStatus?.CancelRequested).HasValue;
        }

        private static RecordActivityTaskHeartbeatRequest HeartbeatRequest(string details, string token)
        {
            return new RecordActivityTaskHeartbeatRequest
            {
                TaskToken = token,
                Details = details
            };
        }
    }
}