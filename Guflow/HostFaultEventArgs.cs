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