namespace NetPlayground
{
    public abstract class WorkflowEvent
    {
        public abstract WorkflowAction Interpret(IWorkflow workflow);
    }
}