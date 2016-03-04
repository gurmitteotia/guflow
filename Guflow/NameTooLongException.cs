using System;

namespace Guflow
{
    public class NameTooLongException : Exception
    {
        public NameTooLongException(string message):base(message)
        {
            
        }
    }
}