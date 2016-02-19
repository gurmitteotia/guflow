using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityCompletedEvent : ActivityEvent
    {
        
        private readonly ActivityTaskCompletedEventAttributes _eventAttributes;
        private readonly IWorkflowContext _workflowContext;

        public ActivityCompletedEvent(HistoryEvent activityCompletedEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = activityCompletedEvent.ActivityTaskCompletedEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
            _workflowContext = new WorkflowContext(allHistoryEvents);
        }

        public string Result { get { return _eventAttributes.Result; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCompleted(this);            
        }

        public override IWorkflowContext WorkflowContext{get { return _workflowContext; }}
    }
}