using System;

namespace Guflow
{
    public class IncompleteEventGraphException :Exception
    {
        public IncompleteEventGraphException(string message):base(message)
        {
        }
    }
}