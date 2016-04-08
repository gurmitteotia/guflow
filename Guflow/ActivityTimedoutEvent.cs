using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityTimedoutEvent : ActivityEvent
    {
        private readonly ActivityTaskTimedOutEventAttributes _eventAttributes;
        private readonly IWorkflowHistoryEvents _workflowHistoryEvents;
        public ActivityTimedoutEvent(HistoryEvent activityTimedoutEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = activityTimedoutEvent.ActivityTaskTimedOutEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
            _workflowHistoryEvents = new WorkflowHistoryEvents(allHistoryEvents);
        }

        public string TimeoutType { get { return _eventAttributes.TimeoutType; } }

        public string Details { get { return _eventAttributes.Details; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityTimedout(this);
        }

        public override IWorkflowHistoryEvents WorkflowHistoryEvents{get { return _workflowHistoryEvents; }}
    }
}