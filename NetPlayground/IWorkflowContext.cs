namespace NetPlayground
{
    public interface IWorkflowContext
    {
        ActivityEvent GetActivityEvent(string activityName, string activityVersion, string positionalName = "");
    }
}