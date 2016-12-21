using System;

namespace Guflow.Decider
{
    public class WorkflowDescriptionMissingException : Exception
    {
        public WorkflowDescriptionMissingException(string message)
            : base(message)
        {
        }
    }
}