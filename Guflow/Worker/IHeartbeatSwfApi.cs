// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Threading;
using System.Threading.Tasks;

namespace Guflow.Worker
{
    internal interface IHeartbeatSwfApi
    {
        Task<bool> RecordHearbeatAsync(string token, string details, CancellationToken cancellationToken);
    }
}