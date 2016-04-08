using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCancelledEvent : ActivityEvent
    {
        private readonly ActivityTaskCanceledEventAttributes _eventAttributes;
        private readonly IWorkflowHistoryEvents _workflowHistoryEvents;
        public ActivityCancelledEvent(HistoryEvent cancelledActivityEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = cancelledActivityEvent.ActivityTaskCanceledEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
            _workflowHistoryEvents = new WorkflowHistoryEvents(allHistoryEvents);
        }

        public string Details{get { return _eventAttributes.Details; }}

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCancelled(this);
        }

        public override IWorkflowHistoryEvents WorkflowHistoryEvents{get { return _workflowHistoryEvents; }}
    }
}