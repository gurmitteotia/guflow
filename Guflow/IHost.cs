// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow
{
    /// <summary>
    /// Represent a host to execute workflows/activities.
    /// </summary>
    public interface IHost
    {
        /// <summary>
        /// Start execution of host on common default task list. Throws exception when default task list is not same for all hosted items(workflows, activities).
        /// </summary>
        void StartExecution();
        /// <summary>
        /// Start excution of host and provide a <see cref="TaskList"/> to poll for new decisions/work.
        /// </summary>
        /// <param name="taskList"></param>
        void StartExecution(TaskList taskList);
        /// <summary>
        /// Stop the execution of host.
        /// </summary>
        void StopExecution();
        /// <summary>
        /// Register the error handler for polling error.
        /// </summary>
        /// <param name="handleError"></param>
        void OnPollingError(HandleError handleError);
        /// <summary>
        /// Register the error handler for response errror.
        /// </summary>
        /// <param name="handleError"></param>
        void OnResponseError(HandleError handleError);

        /// <summary>
        /// Register the generic error handler. It is also acts as fallback error handler. Any unhandled polling and response
        /// error is forwarded to generic error handler.
        /// </summary>
        /// <param name="handleError"></param>
        void OnError(HandleError handleError);

        /// <summary>
        /// Gets or sets the polling identity of host.
        /// </summary>
        string PollingIdentity { get; set; }
    }
}