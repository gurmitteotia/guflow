// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;

namespace Guflow.Decider
{
    /// <summary>
    /// Mark a workflow method to be signal handler.
    /// </summary>
    public class SignalAttribute : Attribute
    {
        /// <summary>
        /// Name of signal. Signal name is compared in case insensitive manner with this property.
        /// </summary>
        public string Name { get; set; }
        internal bool IsFor(string signalName)
        {
            return !string.IsNullOrEmpty(Name) && string.Equals(Name, signalName, StringComparison.OrdinalIgnoreCase);
        }
    }
}