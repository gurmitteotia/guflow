using System;

namespace Guflow.Decider
{
    public class ParentItemMissingException : Exception
    {
        public ParentItemMissingException(string message):base(message)
        {
        }
    }
}