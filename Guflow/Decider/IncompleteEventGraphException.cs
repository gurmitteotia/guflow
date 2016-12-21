using System;

namespace Guflow.Decider
{
    public class IncompleteEventGraphException :Exception
    {
        public IncompleteEventGraphException(string message):base(message)
        {
        }
    }
}