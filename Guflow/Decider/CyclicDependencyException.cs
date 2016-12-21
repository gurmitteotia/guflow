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