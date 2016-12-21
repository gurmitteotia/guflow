namespace Guflow.Decider
{
    internal interface IWorkflow : IWorkflowItems, IWorkflowActions
    {
        IWorkflowEvents WorkflowEvents { get; }
    }
}