using System.Collections.Generic;

namespace NetPlayground
{
    public class ScheduleItemsAction : WorkflowAction
    {
        private readonly IEnumerable<SchedulableItem> _schedulableItems;

        public ScheduleItemsAction(IEnumerable<SchedulableItem> schedulableItems)
        {
            _schedulableItems = schedulableItems;
        }

        protected override WorkflowDecision GetDecision()
        {
            
            //foreach (var schedulableItem in _schedulableItems)
            //{
            //    var parentItems = schedulableItem.Parents;
            //    foreach (var parentItem in parentItems)
            //    {
            //        if(parentItem.IsProcessed())

            //    }
            //}
            //return new ScheduleItemsDecisions(_schedulableItems);
            return null;
        }
    }
}