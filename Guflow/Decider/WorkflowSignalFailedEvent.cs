// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
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

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnWorkflowSignalFailed(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_SIGNAL_WORKFLOW", Cause);
        }
    }
}