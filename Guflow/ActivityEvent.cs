using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class ActivityEvent : WorkflowItemEvent
    {
        private AwsIdentity _identity;
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
            bool foundActivityStartedEvent=false;
            bool foundActivityScheduledEvent=false;
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsActivityStartedEventFor(startedEventId))
                {
                    WorkerIdentity = historyEvent.ActivityTaskStartedEventAttributes.Identity;
                    foundActivityStartedEvent = true;
                }
                else if (historyEvent.IsActivityScheduledEventFor(scheduledEventId))
                {
                    Name = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Name;
                    Version = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Version;
                    PositionalName = historyEvent.ActivityTaskScheduledEventAttributes.Control.FromJson<ActivityScheduleData>().PN;
                    _identity = AwsIdentity.Raw(historyEvent.ActivityTaskScheduledEventAttributes.ActivityId);
                    foundActivityScheduledEvent = true;
                }
            }
            if(!foundActivityStartedEvent)
                throw new IncompleteEventGraphException(string.Format("Can not found activity started event id {0}.", startedEventId));
            if (!foundActivityScheduledEvent)
                throw new IncompleteEventGraphException(string.Format("Can not found activity scheduled event id {0}.", scheduledEventId));
        } 
    }
}