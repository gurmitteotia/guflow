// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when Amazon SWF failed to restart the workflow. It can be raised when Amazon SWF is processing RestartWorkflow action.
    /// </summary>
    public sealed class WorkflowRestartFailedEvent : WorkflowEvent
    {
        internal WorkflowRestartFailedEvent(HistoryEvent @event) 
            : base(@event)
        {
            var attr = @event.ContinueAsNewWorkflowExecutionFailedEventAttributes;
            Cause = attr.Cause;
        }

        /// <summary>
        /// Reason why this event is raised.
        /// </summary>
        public string Cause { get; }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_RESTART_WORKFLOW", Cause);
        }
    }
}