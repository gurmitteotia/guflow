namespace Guflow
{
    public abstract class WorkflowItemEvent : WorkflowEvent
    {
        internal abstract bool IsFor(WorkflowItem workflowItem);
    }
}