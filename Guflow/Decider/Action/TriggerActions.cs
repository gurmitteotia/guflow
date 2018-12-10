// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    /// <summary>
    /// Provides APIs to trigger the scheduling of first joint item.
    /// </summary>
    public class TriggerActions
    {
        private readonly WorkflowItem _triggeringItem;

        internal TriggerActions(WorkflowItem triggeringItem)
        {
            _triggeringItem = triggeringItem;
        }

        /// <summary>
        /// Trigger the scheduling of first joint item. For details please read about Deflow algorithm.
        /// </summary>
        /// <returns></returns>
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