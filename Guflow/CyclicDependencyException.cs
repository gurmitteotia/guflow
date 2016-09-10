using System;

namespace Guflow
{
    public class CyclicDependencyException : Exception
    {
         public CyclicDependencyException(string message):base(message)
         {
         }
    }
}