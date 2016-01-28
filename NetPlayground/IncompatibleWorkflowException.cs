using System;

namespace NetPlayground
{
    public class IncompatibleWorkflowException : Exception
    {
        public IncompatibleWorkflowException(string message):base(message)
        {
            
        }
    }
}
