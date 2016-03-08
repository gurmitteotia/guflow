using System;

namespace Guflow
{
    public class ParentItemMissingException : Exception
    {
        public ParentItemMissingException(string message):base(message)
        {
        }
    }
}