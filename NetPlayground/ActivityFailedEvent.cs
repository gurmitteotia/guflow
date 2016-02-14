using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ActivityFailedEvent : ActivityEvent
    {
        private readonly ActivityTaskFailedEventAttributes _eventAttributes;
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;

        public ActivityFailedEvent(HistoryEvent activityFailedHistoryEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = activityFailedHistoryEvent.ActivityTaskFailedEventAttributes;
            _allHistoryEvents = allHistoryEvents;
            PopulateAttributes(allHistoryEvents);
        }

        public string Identity { get; private set; }
        public string Reason { get { return _eventAttributes.Reason; } }
        public string Detail { get { return _eventAttributes.Details; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityFailed(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { throw new System.NotImplementedException(); }
        }

        private void PopulateAttributes(IEnumerable<HistoryEvent> allHistoryEvents)
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
