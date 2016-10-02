using System;

namespace Guflow
{
    public class WorkflowDescriptionMissingException : Exception
    {
        public WorkflowDescriptionMissingException(string message)
            : base(message)
        {
        }
    }
}