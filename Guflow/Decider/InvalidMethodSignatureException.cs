using System;

namespace Guflow.Decider
{
    public class InvalidMethodSignatureException : Exception
    {
        public InvalidMethodSignatureException(string message):base(message)
        {
        } 
    }
}