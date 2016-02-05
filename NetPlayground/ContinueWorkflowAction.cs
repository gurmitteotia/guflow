using System.Collections.Generic;

namespace NetPlayground
{
    public class ContinueWorkflowAction : WorkflowAction
    {
        private readonly WorkflowEvent _completedWorkflowEvent;
        private readonly HashSet<SchedulableItem> _allSchedulableItems;

        public ContinueWorkflowAction(WorkflowEvent completedWorkflowEvent, HashSet<SchedulableItem> allSchedulableItems)
        {
            _completedWorkflowEvent = completedWorkflowEvent;
            _allSchedulableItems = allSchedulableItems;
        }


        protected override WorkflowDecision GetDecision()
        {
            var completedSchedulableItem = _completedWorkflowEvent.FindSchedulableItemIn(_allSchedulableItems);

            if (completedSchedulableItem == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find schedulable item {0} in workflow.",_completedWorkflowEvent));

            var childItems = _allSchedulableItems.GetChildernOf(completedSchedulableItem);
            return new ScheduleItemsDecisions(childItems);
        }
    }
}