// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Worker
{
    public class ActivityDescriptionMissingException : Exception
    {
        public ActivityDescriptionMissingException(string message): base(message)
        {
        }
    }
}