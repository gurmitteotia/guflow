// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Worker
{
    /// <summary>
    /// Mark a method in activity to be executed when it start.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ActivityMethodAttribute : Attribute
    {
    }
}