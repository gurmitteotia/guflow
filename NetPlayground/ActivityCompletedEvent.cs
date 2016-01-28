using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ActivityCompletedEvent : WorkflowEvent
    {
        private readonly ActivityTaskCompletedEventAttributes _eventAttributes;
        public ActivityCompletedEvent(HistoryEvent activityCompletedEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = activityCompletedEvent.ActivityTaskCompletedEventAttributes;
            PopulateHistoryEvents(allHistoryEvents);
        }
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string PositionalName { get; private set; }

        public string Identity { get; private set; }
        public string Result { get { return _eventAttributes.Result; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCompleted(this);            
        }

        private void PopulateHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.EventId == _eventAttributes.StartedEventId && historyEvent.EventType == EventType.ActivityTaskStarted)
                {
                     Identity = historyEvent.ActivityTaskStartedEventAttributes.Identity;
                }
                else if (historyEvent.EventId == _eventAttributes.ScheduledEventId && historyEvent.EventType == EventType.ActivityTaskScheduled)
                {
                    Name = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Name;
                    Version = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Version;
                    PositionalName = historyEvent.ActivityTaskScheduledEventAttributes.Control.FromJson<ScheduleData>().PN;
                }
            }
        }
    }
}