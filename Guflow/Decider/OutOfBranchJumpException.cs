// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace Guflow.Decider
{
    [Serializable]
    public class OutOfBranchJumpException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public OutOfBranchJumpException()
        {
        }

        public OutOfBranchJumpException(string message) : base(message)
        {
        }

        public OutOfBranchJumpException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}