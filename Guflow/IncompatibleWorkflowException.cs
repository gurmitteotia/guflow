using System;

namespace Guflow
{
    public class IncompatibleWorkflowException : Exception
    {
        public IncompatibleWorkflowException(string message):base(message)
        {
        }
    }
}
