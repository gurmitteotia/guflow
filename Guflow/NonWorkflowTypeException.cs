using System;

namespace Guflow
{
    public class NonWorkflowTypeException : Exception
    {
        public NonWorkflowTypeException(string message):base(message)
        {
        }
    }
}