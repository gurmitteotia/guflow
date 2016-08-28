using System;

namespace Guflow
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple = false,Inherited = true)]
    public class WorkflowEventAttribute : Attribute
    {
    }
}