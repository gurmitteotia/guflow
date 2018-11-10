// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when Amazon SWF can not complete the workflow.
    /// </summary>
    public sealed class WorkflowCompletionFailedEvent : WorkflowEvent
    {
        private readonly CompleteWorkflowExecutionFailedEventAttributes _eventAttributes;

        internal WorkflowCompletionFailedEvent(HistoryEvent workflowCompletionFailureEvent) :base(workflowCompletionFailureEvent.EventId)
        {
            _eventAttributes = workflowCompletionFailureEvent.CompleteWorkflowExecutionFailedEventAttributes;
        }

        /// <summary>
        /// Reason why workflow was not completed.
        /// </summary>
        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_COMPLETE_WORKFLOW", Cause);
        }
    }
}