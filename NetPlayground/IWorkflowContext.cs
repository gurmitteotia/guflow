namespace NetPlayground
{
    public interface IWorkflowContext
    {
        WorkflowEvent GetActivityEvent(string activityName, string activityVersion, string positionalName = "");
    }
}