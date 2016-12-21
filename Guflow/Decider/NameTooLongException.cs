using System;

namespace Guflow.Decider
{
    public class NameTooLongException : Exception
    {
        public NameTooLongException(string message):base(message)
        {
            
        }
    }
}