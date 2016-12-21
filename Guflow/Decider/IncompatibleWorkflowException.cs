using System;

namespace Guflow.Decider
{
    public class IncompatibleWorkflowException : Exception
    {
        public IncompatibleWorkflowException(string message):base(message)
        {
        }
    }
}
