using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class RecordMarkerFailedEvent : WorkflowEvent
    {
        private RecordMarkerFailedEventAttributes _eventAttributes;
        internal RecordMarkerFailedEvent(HistoryEvent recordMarkerFailedEvent): base(recordMarkerFailedEvent.EventId)
        {
            _eventAttributes = recordMarkerFailedEvent.RecordMarkerFailedEventAttributes;
        }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            throw new System.NotImplementedException();
        }
    }
}