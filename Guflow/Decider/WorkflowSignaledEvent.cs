// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent a signal event, generated as a result of sending the signal to a workflow.
    /// </summary>
    public class WorkflowSignaledEvent : WorkflowEvent
    {
        private readonly WorkflowExecutionSignaledEventAttributes _eventAttributes;
        internal WorkflowSignaledEvent(HistoryEvent signaledEvent) : base(signaledEvent.EventId)
        {
            _eventAttributes = signaledEvent.WorkflowExecutionSignaledEventAttributes;
        }
        /// <summary>
        /// Gets signal name.
        /// </summary>
        public string SignalName => _eventAttributes.SignalName;

        /// <summary>
        /// Get signal input.
        /// </summary>
        public string Input => _eventAttributes.Input;

        /// <summary>
        /// Get the workflow run id of the workflow which has send this signal.
        /// </summary>
        public string ExternalWorkflowRunid => 
            _eventAttributes.ExternalWorkflowExecution?.RunId;

        /// <summary>
        /// Get the workflow id of the workflow which has send this signal.
        /// </summary>
        public string ExternalWorkflowId => 
            _eventAttributes.ExternalWorkflowExecution?.WorkflowId;

        /// <summary>
        /// Indicate if this signal was send by a workflow.
        /// </summary>
        public bool IsSentByWorkflow => _eventAttributes.ExternalWorkflowExecution != null;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnWorkflowSignaled(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.Ignore();
        }
    }
}