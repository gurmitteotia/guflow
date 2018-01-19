// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    public class CyclicDependencyException : Exception
    {
         public CyclicDependencyException(string message):base(message)
         {
         }
    }
}