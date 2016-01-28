using System;

namespace NetPlayground
{
    public class ParentItemNotFoundException : Exception
    {
        public ParentItemNotFoundException(string message):base(message)
        {
        }
    }
}