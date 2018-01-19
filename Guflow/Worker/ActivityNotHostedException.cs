// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Worker
{
    public class ActivityNotHostedException : Exception
    {
        public ActivityNotHostedException(string message) : base(message)
        {
        }
    }
}