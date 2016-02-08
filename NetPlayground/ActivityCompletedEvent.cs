using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ActivityCompletedEvent : ActivityEvent
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly ActivityTaskCompletedEventAttributes _eventAttributes;
        public ActivityCompletedEvent(HistoryEvent activityCompletedEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
            _eventAttributes = activityCompletedEvent.ActivityTaskCompletedEventAttributes;
            PopulateHistoryEvents(allHistoryEvents);
        }

        public string Identity { get; private set; }
        public string Result { get { return _eventAttributes.Result; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCompleted(this);            
        }

        public override IWorkflowContext WorkflowContext
        {
            get { return new WorkflowContext(_allHistoryEvents); }
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

        public override bool IsProcessed
        {
            get { return true; }
        }
    }
}