// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message):base(message)
        {
        } 
    }
}