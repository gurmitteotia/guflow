namespace Guflow
{
    public interface IFluentWorkflowItem<T>
    {
        T DependsOn(string timerName);
        T DependsOn(string activityName, string activityVersion, string positionalName = "");
    }
}