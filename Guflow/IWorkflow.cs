namespace Guflow
{
    internal interface IWorkflow : IWorkflowItems, IWorkflowActions
    {
        IWorkflowHistoryEvents CurrentHistoryEvents { get; }
    }
}