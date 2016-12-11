namespace Guflow
{
    internal interface IWorkflow : IWorkflowItems, IWorkflowActions
    {
        IWorkflowEvents WorkflowEvents { get; }
    }
}