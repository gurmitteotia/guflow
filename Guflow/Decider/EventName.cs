// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    /// <summary>
    /// Represent a workflow specific event which can use in <see cref="WorkflowEventAttribute"/>.
    /// </summary>
    public enum EventName
    {
        /// <summary>
        /// Raised when workflow is started.
        /// </summary>
        WorkflowStarted,
        /// <summary>
        /// Raised when this workflow has received a signal
        /// </summary>
        Signal,
        /// <summary>
        /// Raised when this workflow failed to deliver the signal to other workflow.
        /// </summary>
        SignalFailed,
        /// <summary>
        /// Raised when this workflow has received the request to cancel itself.
        /// </summary>
        CancelRequest,
        /// <summary>
        /// Raised when this workflow fails to send the cancel request to external workflow.
        /// </summary>
        CancelRequestFailed,
        /// <summary>
        /// Raised when SWF fails to process CompleteWorkflow action.
        /// </summary>
        CompletionFailed,
        /// <summary>
        /// Raised when SWF fails to process the FailWorkflow action.
        /// </summary>
        FailureFailed,
        /// <summary>
        /// Raised when SWF fails to process the CancelWorkflow action.
        /// </summary>
        CancellationFailed,

        /// <summary>
        /// Raised when SWF fails to process RecordMarker action.
        /// </summary>
        RecordMarkerFailed
    }
}