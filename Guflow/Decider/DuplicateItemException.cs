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