using System;

namespace Guflow
{
    public class AmbiguousWorkflowMethodException : Exception
    {
        public AmbiguousWorkflowMethodException(string message):base(message)
        {
        }
    }
}