namespace Guflow.Decider
{
    public interface IFluentWorkflowItem<T>
    {
        T After(string timerName);
        T After(string activityName, string activityVersion, string positionalName = "");
    }
}