using System;

namespace Guflow
{
    public class WorkflowDeprecatedException : Exception
    {
        public WorkflowDeprecatedException(string message):base(message)
        {
        }
    }
}