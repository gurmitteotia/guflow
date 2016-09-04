using System;

namespace Guflow
{
    public class ActivityNotActiveException : Exception
    {
         public ActivityNotActiveException(string message):base(message)
         {
         }
    }
}