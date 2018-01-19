// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow
{
    public enum HostStatus
    {
        /// <summary>
        /// Host is initialized and has not started execution.
        /// </summary>
        Initialized,
        /// <summary>
        /// Host is executing hosted workflows/activities.
        /// </summary>
        Executing,
        /// <summary>
        /// Host has stopped execution.
        /// </summary>
        Stopped,

        /// <summary>
        /// Host is in faulted state. It is no more executing the hosted workflows and activities.
        /// </summary>
        Faulted
    }
}