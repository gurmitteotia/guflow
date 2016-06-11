namespace Guflow
{
    public abstract class WorkflowItemEvent : WorkflowEvent
    {
        protected AwsIdentity AwsIdentity;
        internal bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(AwsIdentity);
        }
        public bool IsActive { get; protected set; }
    }
}