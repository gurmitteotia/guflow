using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class WorkflowSignaledEvent : WorkflowEvent
    {
        private readonly WorkflowExecutionSignaledEventAttributes _eventAttributes;
        internal WorkflowSignaledEvent(HistoryEvent signaledEvent) : base(signaledEvent.EventId)
        {
            _eventAttributes = signaledEvent.WorkflowExecutionSignaledEventAttributes;
        }

        public string SignalName { get { return _eventAttributes.SignalName; } }
        public string Input { get { return _eventAttributes.Input; }}

        public string ExternalWorkflowRunid
        {
            get
            {
                return _eventAttributes.ExternalWorkflowExecution != null
                    ? _eventAttributes.ExternalWorkflowExecution.RunId
                    : null;
            }
        }
        public string ExternalWorkflowId
        {
            get
            {
                return _eventAttributes.ExternalWorkflowExecution != null
                    ? _eventAttributes.ExternalWorkflowExecution.WorkflowId
                    : null;
            }
        }

        internal override WorkflowAction Interpret(IWorkflowActions workflowActions)
        {
            return workflowActions.OnWorkflowSignaled(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.Ignore();
        }
    }
}