using System;

namespace Guflow
{
    public class InvalidMethodSignatureException : Exception
    {
        public InvalidMethodSignatureException(string message):base(message)
        {
        } 
    }
}