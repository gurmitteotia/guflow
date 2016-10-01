using System;

namespace Guflow
{
    public class AttributeMissingException : Exception
    {
        public AttributeMissingException(string message)
            : base(message)
        {
        }
    }
}