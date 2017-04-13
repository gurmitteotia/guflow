namespace Guflow.Decider
{
    public interface IFluentWorkflowItem<out T>
    {
        T After(string timerName);
        T After(string activityName, string activityVersion, string positionalName = "");
    }
}