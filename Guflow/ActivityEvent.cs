using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class ActivityEvent : WorkflowItemEvent
    {
        private Identity _identity;
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string PositionalName { get; private set; }
        public string WorkerIdentity { get; private set; }

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_identity);
        }

        protected void PopulateActivityFrom(IEnumerable<HistoryEvent> allHistoryEvents, long startedEventId, long scheduledEventId)
        {
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsActivityStartedEventFor(startedEventId))
                {
                    WorkerIdentity = historyEvent.ActivityTaskStartedEventAttributes.Identity;
                }
                else if (historyEvent.IsActivityScheduledEventFor(scheduledEventId))
                {
                    Name = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Name;
                    Version = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Version;
                    PositionalName = historyEvent.ActivityTaskScheduledEventAttributes.Control.FromJson<ActivityScheduleData>().PN;
                    _identity = Identity.New(Name,Version,PositionalName);
                }
            }
        } 
    }
}