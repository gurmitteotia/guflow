using System;

namespace Guflow
{
    public class TimerNotActiveException : Exception
    {
        public TimerNotActiveException(string message):base(message)
        {
        }
    }
}