namespace Guflow
{
    public abstract class WorkflowEvent
    {
        internal abstract WorkflowAction Interpret(IWorkflow workflow);
    }
}