// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    public enum EventName
    {
        WorkflowStarted,
        Signal,
        SignalFailed,
        CancelRequest,
        CancelRequestFailed,
        CompletionFailed,
        FailureFailed,
        CancellationFailed,
        RecordMarkerFailed
    }
}