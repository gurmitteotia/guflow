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