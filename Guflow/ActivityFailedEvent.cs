using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityFailedEvent : ActivityEvent
    {
        private readonly ActivityTaskFailedEventAttributes _eventAttributes;
        private readonly IWorkflowHistoryEvents _workflowHistoryEvents;

        internal ActivityFailedEvent(HistoryEvent activityFailedHistoryEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = activityFailedHistoryEvent.ActivityTaskFailedEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
            _workflowHistoryEvents = new WorkflowHistoryEvents(allHistoryEvents);
        }

        public string Reason { get { return _eventAttributes.Reason; } }
        public string Detail { get { return _eventAttributes.Details; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityFailed(this);
        }
        public override IWorkflowHistoryEvents WorkflowHistoryEvents{get { return _workflowHistoryEvents; }}
    }
}
