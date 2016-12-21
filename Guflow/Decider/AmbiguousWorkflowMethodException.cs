using System;

namespace Guflow.Decider
{
    public class AmbiguousWorkflowMethodException : Exception
    {
        public AmbiguousWorkflowMethodException(string message):base(message)
        {
        }
    }
}