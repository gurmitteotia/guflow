// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Guflow.Properties;

namespace Guflow
{
    internal class HostState
    {
        private HostStatus _status;
        private readonly object _lock = new object();

        public HostState()
        {
            _status = HostStatus.Initialized;
        }

        public void Start()
        {
            lock (_lock)
            {
                if(_status == HostStatus.Executing)
                    throw new InvalidOperationException(Resources.Host_already_excuting);
                if(_status == HostStatus.Faulted)
                    throw new InvalidOperationException(Resources.Host_is_faulted);
                if (_status == HostStatus.Stopped)
                    throw new InvalidOperationException(Resources.Host_is_stopped);
                _status = HostStatus.Executing;
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_status != HostStatus.Faulted)
                    _status = HostStatus.Stopped;
            }
        }

        public void Fault()
        {
            lock (_lock)
            {
                _status = HostStatus.Faulted;
            }
        }

        public bool CanBeStopped()
        {
            return _status == HostStatus.Initialized || _status == HostStatus.Executing;
        }

        public HostStatus Status => _status;
    }
}