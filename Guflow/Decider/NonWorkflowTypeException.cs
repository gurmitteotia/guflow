// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    public class NonWorkflowTypeException : Exception
    {
        public NonWorkflowTypeException(string message):base(message)
        {
        }
    }
}