using System;

namespace Guflow.Worker
{
    /// <summary>
    /// Mark a method in activity to be executed when it start.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ExecuteAttribute : Attribute
    {
    }
}