using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class ActivityEvent : WorkflowEvent
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string PositionalName { get; private set; }
        public string Identity { get; private set; }

        public bool Has(string name, string version, string positionalName)
        {
            return string.Equals(Name, name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Version, version, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PositionalName, positionalName);
        }

        protected void PopulateActivityFrom(IEnumerable<HistoryEvent> allHistoryEvents, long startedEventId, long scheduledEventId)
        {
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsActivityStartedEventFor(startedEventId))
                {
                    Identity = historyEvent.ActivityTaskStartedEventAttributes.Identity;
                }
                else if (historyEvent.IsActivityScheduledEventFor(scheduledEventId))
                {
                    Name = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Name;
                    Version = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Version;
                    PositionalName = historyEvent.ActivityTaskScheduledEventAttributes.Control.FromJson<ScheduleData>().PN;
                }
            }
        } 
    }
}