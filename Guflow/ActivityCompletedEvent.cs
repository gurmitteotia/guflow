using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCompletedEvent : ActivityEvent
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly ActivityTaskCompletedEventAttributes _eventAttributes;
        public ActivityCompletedEvent(HistoryEvent activityCompletedEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
            _eventAttributes = activityCompletedEvent.ActivityTaskCompletedEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);

        }

        public string Result { get { return _eventAttributes.Result; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCompleted(this);            
        }

        public override IWorkflowContext WorkflowContext
        {
            get { return new WorkflowContext(_allHistoryEvents); }
        }

     
    }
}