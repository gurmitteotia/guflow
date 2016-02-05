using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ActivityFailedEvent : WorkflowEvent
    {
        private readonly HistoryEvent _activityFailedHistoryEvent;

        public ActivityFailedEvent(HistoryEvent activityFailedHistoryEvent)
        {
            _activityFailedHistoryEvent = activityFailedHistoryEvent;
        }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityFailed(this);
        }

        internal override SchedulableItem FindSchedulableItemIn(HashSet<SchedulableItem> allSchedulableItems)
        {
            throw new System.NotImplementedException();
        }
    }
}
