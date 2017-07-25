using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class ActivityCancelledEvent : ActivityEvent
    {
        private readonly ActivityTaskCanceledEventAttributes _eventAttributes;
        internal ActivityCancelledEvent(HistoryEvent cancelledActivityEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(cancelledActivityEvent.EventId)
        {
            _eventAttributes = cancelledActivityEvent.ActivityTaskCanceledEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }
        public string Details{get { return _eventAttributes.Details; }}

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnActivityCancelled(this);
        }

        internal bool IsCancelledEventFor(long cancelRequestedEventId)
        {
            return _eventAttributes.LatestCancelRequestedEventId == cancelRequestedEventId;
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.CancelWorkflow(Details);
        }
    }
}