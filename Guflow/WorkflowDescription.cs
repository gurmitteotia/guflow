namespace Guflow
{
    public interface WorkflowDescription
    {
        string Name { get; set; }
        string Version { get; set; }
        string Description { get; set; }
        string DefaultTaskListName { get; set; }
        string DefaultChildPolicy { get; set; }
        string DefaultLambdaRole { get; set; }
        uint DefaultExecutionStartToCloseTimeoutInSeconds { get; set; }
        uint DefaultTaskStartToCloseTimeoutInSeconds { get; set; }
        int DefaultTaskPriority { get; set; }
        string DefaultExecutionStartToCloseTimeout { get; }
        string DefaultTaskStartToCloseTimeout { get; }
    }
}