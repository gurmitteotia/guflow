using System;

namespace Guflow.Worker
{
    public class NonActivityTypeException : Exception
    {
        public NonActivityTypeException(string message) : base(message)
        {
            
        }
    }
}