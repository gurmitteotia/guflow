using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class WorkflowSignalFailedEvent : WorkflowEvent
    {
        private readonly SignalExternalWorkflowExecutionFailedEventAttributes _eventAttributes;
        internal WorkflowSignalFailedEvent(HistoryEvent workflowSignalFailedEvent): base(workflowSignalFailedEvent.EventId)
        {
            _eventAttributes = workflowSignalFailedEvent.SignalExternalWorkflowExecutionFailedEventAttributes;
        }

        public string Cause { get { return _eventAttributes.Cause; } }
        public string WorkflowId { get { return _eventAttributes.WorkflowId; } }
        public string RunId { get { return _eventAttributes.RunId; } }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnWorkflowSignalFailed(this);
        }
    }
}