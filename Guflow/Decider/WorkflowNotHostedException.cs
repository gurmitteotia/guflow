using System;

namespace Guflow.Decider
{
    public class WorkflowNotHostedException : Exception
    {
         public WorkflowNotHostedException(string message):base(message)
         {
         }
    }
}