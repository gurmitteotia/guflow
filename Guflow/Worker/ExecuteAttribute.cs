using System;

namespace Guflow.Worker
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ExecuteAttribute : Attribute
    {
    }
}