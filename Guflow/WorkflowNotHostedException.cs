using System;

namespace Guflow
{
    public class WorkflowNotHostedException : Exception
    {
         public WorkflowNotHostedException(string message):base(message)
         {
         }
    }
}