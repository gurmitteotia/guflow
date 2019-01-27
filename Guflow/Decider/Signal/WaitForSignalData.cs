namespace Guflow.Decider
{
    internal class WaitForSignalData
    {
        public string ScheduleId;
        public long TriggerEventId;
        public string[] SignalNames;
        public SignalWaitType WaitType;
        public SignalNextAction NextAction;
    }

    internal enum SignalWaitType
    {
        Any,
        All
    }

    internal enum SignalNextAction
    {
        Continue,
        Reschedule
    }
}