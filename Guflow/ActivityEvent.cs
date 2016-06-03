using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class ActivityEvent : WorkflowItemEvent
    {
        private string _activityName;
        private string _activityVersion;
        private string _activityPositionalName;
        public string WorkerIdentity { get; private set; }

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
                    _activityName = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Name;
                    _activityVersion = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Version;
                    _activityPositionalName = historyEvent.ActivityTaskScheduledEventAttributes.Control.FromJson<ActivityScheduleData>().PN;
                    AwsIdentity = AwsIdentity.Raw(historyEvent.ActivityTaskScheduledEventAttributes.ActivityId);
                    foundActivityScheduledEvent = true;
                }
            }
            if(!foundActivityStartedEvent)
                throw new IncompleteEventGraphException(string.Format("Can not found activity started event id {0}.", startedEventId));
            if (!foundActivityScheduledEvent)
                throw new IncompleteEventGraphException(string.Format("Can not found activity scheduled event id {0}.", scheduledEventId));
        }

        public override string ToString()
        {
            return string.Format("{0} for activity name {1}, version {2} and positional name {3}", GetType().Name, _activityName, _activityVersion, _activityPositionalName);
        }
    }
}