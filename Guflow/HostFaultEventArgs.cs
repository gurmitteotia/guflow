// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow
{
    public class HostFaultEventArgs : EventArgs
    {
        public Exception Exception { get; }

        internal HostFaultEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}