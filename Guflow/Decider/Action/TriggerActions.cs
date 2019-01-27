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

        internal WorkflowAction FirstJoint(WorkflowItem jumptedItem)
        {
            var trigger = new TriggerWorkflowAction(_triggeringItem);
            trigger.SetJumpedItem(jumptedItem);
            return trigger;
        }
    }
}