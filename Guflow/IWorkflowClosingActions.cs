namespace Guflow
{
    internal interface IWorkflowClosingActions
    {
        WorkflowAction OnCompletion(string result, bool proposal);

        WorkflowAction OnFailure(string reason, string detail);

        WorkflowAction OnCancellation(string detail);
    }
}