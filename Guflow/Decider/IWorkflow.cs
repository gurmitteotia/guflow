namespace Guflow.Decider
{
    internal interface IWorkflow : IWorkflowItems, IWorkflowActions, IWorkflowDefaultActions
    {
        IWorkflowEvents WorkflowEvents { get; }
    }
}