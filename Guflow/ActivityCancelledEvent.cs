using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCancelledEvent : ActivityEvent
    {
        private readonly ActivityTaskCanceledEventAttributes _eventAttributes;
        private readonly IWorkflowContext _workflowContext;
        public ActivityCancelledEvent(HistoryEvent cancelledActivityEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = cancelledActivityEvent.ActivityTaskCanceledEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
            _workflowContext = new WorkflowContext(allHistoryEvents);
        }

        public string Details{get { return _eventAttributes.Details; }}

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCancelled(this);
        }

        public override IWorkflowContext WorkflowContext{get { return _workflowContext; }}
    }
}