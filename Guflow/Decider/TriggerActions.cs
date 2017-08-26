namespace Guflow.Decider
{
    public class TriggerActions
    {
        private readonly WorkflowItem _triggeringItem;

        internal TriggerActions(WorkflowItem triggeringItem)
        {
            _triggeringItem = triggeringItem;
        }

        public WorkflowAction FirstJoint()
        {
            return  new TriggerWorkflowAction(_triggeringItem);
        }

        internal WorkflowAction FirstJoint(IWorkflowItem beforeItem)
        {
            var workflowItem = beforeItem as WorkflowItem;
            Ensure.NotNull(workflowItem, nameof(beforeItem));

            var trigger = new TriggerWorkflowAction(_triggeringItem);
            trigger.SetJumpedItem(workflowItem);
            return trigger;
        }
    }
}