// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public abstract class ActivityResponse
    {
        public static readonly ActivityResponse Defer = new DeferredResponse();

        internal abstract Task SendAsync(string taskToken, IAmazonSimpleWorkflow simpleWorkflow, CancellationToken cancellationToken);

        private class DeferredResponse : ActivityResponse
        {
            internal override Task SendAsync(string taskToken, IAmazonSimpleWorkflow simpleWorkflow,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(false);
            }
        }
    }
}