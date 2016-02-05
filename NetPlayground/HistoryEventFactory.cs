using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class HistoryEventFactory
    {
        public static IEnumerable<HistoryEvent> CreateActivityCompletedEventGraph(string activityName, string version, string positionalName, string identity, string result)
        {
            var historyEvents = new List<HistoryEvent>();
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCompleted,
                EventId = 9,
                ActivityTaskCompletedEventAttributes = new ActivityTaskCompletedEventAttributes()
                {
                    Result = result,
                    StartedEventId = 8,
                    ScheduledEventId = 7
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = 8,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = 7

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = 7,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityName, Version = version },
                    Control = (new ScheduleData() { PN = positionalName }).ToJson()
                }
            });
            return historyEvents;
        } 
    }
}