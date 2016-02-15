namespace Guflow
{
    public abstract class WorkflowEvent
    {
        public abstract WorkflowAction Interpret(IWorkflow workflow);

        public abstract IWorkflowContext WorkflowContext { get; }
    }
}