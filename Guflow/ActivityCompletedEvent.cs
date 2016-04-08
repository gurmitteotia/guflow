using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCompletedEvent : ActivityEvent
    {
        
        private readonly ActivityTaskCompletedEventAttributes _eventAttributes;
        private readonly IWorkflowHistoryEvents _workflowHistoryEvents;

        public ActivityCompletedEvent(HistoryEvent activityCompletedEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = activityCompletedEvent.ActivityTaskCompletedEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
            _workflowHistoryEvents = new WorkflowHistoryEvents(allHistoryEvents);
        }

        public string Result { get { return _eventAttributes.Result; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCompleted(this);            
        }

        public override IWorkflowHistoryEvents WorkflowHistoryEvents{get { return _workflowHistoryEvents; }}
    }
}