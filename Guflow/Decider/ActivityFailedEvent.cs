using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class ActivityFailedEvent : ActivityEvent
    {
        private readonly ActivityTaskFailedEventAttributes _eventAttributes;

        internal ActivityFailedEvent(HistoryEvent activityFailedHistoryEvent, IEnumerable<HistoryEvent> allHistoryEvents) : base(activityFailedHistoryEvent.EventId)
        {
            _eventAttributes = activityFailedHistoryEvent.ActivityTaskFailedEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }

        public string Reason { get { return _eventAttributes.Reason; } }
        public string Details { get { return _eventAttributes.Details; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnActivityFailure(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow(Reason, Details);
        }
    }
}
