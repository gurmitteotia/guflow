// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;

namespace Guflow.Decider
{
    /// <summary>
    /// Throws when a workflow item is resumed on a signal it is not waiting for. 
    /// </summary>
    public class SignalResumeException : Exception
    {
        public SignalResumeException(string msg):base(msg)
        {
        }
    }
}