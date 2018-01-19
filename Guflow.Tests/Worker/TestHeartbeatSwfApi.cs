// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Worker;

namespace Guflow.Tests.Worker
{
    public class TestHeartbeatSwfApi : IHeartbeatSwfApi
    {
        private readonly Func<bool> _response;
        private readonly int _setEventOnCalledTimes;
        private readonly ManualResetEvent _event = new ManualResetEvent(false);

        public TestHeartbeatSwfApi(Func<bool> response, int setEventOnCalledTimes =1)
        {
            _response = response;
            _setEventOnCalledTimes = setEventOnCalledTimes;
        }

        public Task<bool> RecordHearbeatAsync(string token, string details, CancellationToken cancellationToken)
        {
            HearbeatRecorded = true;
            Details = details;
            if(++HearbeatRecordedTimes==_setEventOnCalledTimes)
                _event.Set();
            return Task.FromResult(_response());
        }

        public bool Wait(int milliseconds)
        {
            return _event.WaitOne(milliseconds);
        }
        public bool HearbeatRecorded;
        public string Details;
        public int HearbeatRecordedTimes;
    }

}