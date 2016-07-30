using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityTimedoutEvent : ActivityEvent
    {
        private readonly ActivityTaskTimedOutEventAttributes _eventAttributes;
        internal ActivityTimedoutEvent(HistoryEvent activityTimedoutEvent, IEnumerable<HistoryEvent> allHistoryEvents) : base(activityTimedoutEvent.EventId)
        {
            _eventAttributes = activityTimedoutEvent.ActivityTaskTimedOutEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }

        public string TimeoutType { get { return _eventAttributes.TimeoutType; } }

        public string Details { get { return _eventAttributes.Details; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnActivityTimeout(this);
        }
    }
}