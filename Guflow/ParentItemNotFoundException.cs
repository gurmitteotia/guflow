using System;

namespace Guflow
{
    public class ParentItemNotFoundException : Exception
    {
        public ParentItemNotFoundException(string message):base(message)
        {
        }
    }
}