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
            var eventIds = EventIds.NewEventIds;
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskCompleted,
                EventId = eventIds.CompletedId,
                ActivityTaskCompletedEventAttributes = new ActivityTaskCompletedEventAttributes()
                {
                    Result = result,
                    StartedEventId = eventIds.StartedId,
                    ScheduledEventId = eventIds.ScheduledId
                }
            });

            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskStarted,
                EventId = eventIds.StartedId,
                ActivityTaskStartedEventAttributes = new ActivityTaskStartedEventAttributes()
                {
                    Identity = identity,
                    ScheduledEventId = eventIds.ScheduledId

                }
            });
            historyEvents.Add(new HistoryEvent()
            {
                EventType = EventType.ActivityTaskScheduled,
                EventId = eventIds.ScheduledId,
                ActivityTaskScheduledEventAttributes = new ActivityTaskScheduledEventAttributes()
                {
                    ActivityType = new ActivityType() { Name = activityName, Version = version },
                    Control = (new ScheduleData() { PN = positionalName }).ToJson()
                }
            });
            return historyEvents;
        }

        private class EventIds
        {
            private static long _seed = long.MaxValue;
            private readonly long _completedId;
            private EventIds(long completedId)
            {
                _completedId = completedId;
            }

            public static EventIds NewEventIds
            {
                get
                {
                    _seed -= 10;
                    return new EventIds(_seed);
                }
            }

            public long CompletedId
            {
                get { return _completedId;}
            }
            public long StartedId
            {
                get { return _completedId - 1; }
            }

            public long ScheduledId
            {
                get { return _completedId - 2; }
            }
        }
    }
}