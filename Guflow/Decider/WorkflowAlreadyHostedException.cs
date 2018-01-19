// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace Guflow.Decider
{
    [Serializable]
    public class WorkflowAlreadyHostedException : Exception
    {
        public WorkflowAlreadyHostedException()
        {
        }

        public WorkflowAlreadyHostedException(string message) : base(message)
        {
        }

        public WorkflowAlreadyHostedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected WorkflowAlreadyHostedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}