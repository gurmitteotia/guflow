using System;

namespace Guflow.Decider
{
    public class NonWorkflowTypeException : Exception
    {
        public NonWorkflowTypeException(string message):base(message)
        {
        }
    }
}