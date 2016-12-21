using System;

namespace Guflow.Decider
{
    public class WorkflowItemNotFoundException : Exception
    {
        public WorkflowItemNotFoundException(string message):base(message)
        {
        }
    }
}