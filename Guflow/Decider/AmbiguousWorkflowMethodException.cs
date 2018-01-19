// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    public class AmbiguousWorkflowMethodException : Exception
    {
        public AmbiguousWorkflowMethodException(string message):base(message)
        {
        }
    }
}