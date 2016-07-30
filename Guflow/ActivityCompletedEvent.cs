using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCompletedEvent : ActivityEvent
    {
        private readonly ActivityTaskCompletedEventAttributes _eventAttributes;
        internal ActivityCompletedEvent(HistoryEvent activityCompletedEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(activityCompletedEvent.EventId)
        {
            _eventAttributes = activityCompletedEvent.ActivityTaskCompletedEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }

        public string Result { get { return _eventAttributes.Result; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnActivityCompletion(this);            
        }
    }
}