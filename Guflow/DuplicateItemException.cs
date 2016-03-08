using System;

namespace Guflow
{
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message):base(message)
        {
        } 
    }
}