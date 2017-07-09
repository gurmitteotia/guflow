using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class RecordMarkerFailedEvent : WorkflowEvent
    {
        private readonly RecordMarkerFailedEventAttributes _eventAttributes;
        internal RecordMarkerFailedEvent(HistoryEvent recordMarkerFailedEvent): base(recordMarkerFailedEvent.EventId)
        {
            _eventAttributes = recordMarkerFailedEvent.RecordMarkerFailedEventAttributes;
        }

        public string MarkerName { get { return _eventAttributes.MarkerName; } }
        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnRecordMarkerFailed(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_RECORD_MARKER", Cause);
        }
    }
}