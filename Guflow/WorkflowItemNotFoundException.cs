using System;

namespace Guflow
{
    public class WorkflowItemNotFoundException : Exception
    {
        public WorkflowItemNotFoundException(string message):base(message)
        {
        }
    }
}