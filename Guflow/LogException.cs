using System;
using System.Runtime.Serialization;

namespace Guflow
{
    [Serializable]
    public class LogException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public LogException()
        {
        }

        public LogException(string message) : base(message)
        {
        }

        public LogException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LogException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}