using System.Collections.Generic;

namespace Guflow
{
    public abstract class WorkflowAction
    {
        internal abstract IEnumerable<WorkflowDecision> GetDecisions();

        internal static WorkflowAction FailWorkflow(string reason, string detail)
        {
            return new GenericWorkflowAction(new FailWorkflowDecision(reason,detail));
        }
        internal static WorkflowAction CompleteWorkflow(string result)
        {
            return new GenericWorkflowAction(new CompleteWorkflowDecision(result));
        }
        internal static WorkflowAction CancelWorkflow(string detail)
        {
            return new GenericWorkflowAction(new CancelWorkflowDecision(detail));
        }
        internal static RescheduleWorkflowAction Reschedule(WorkflowItem workflowItem)
        {
            return new RescheduleWorkflowAction(workflowItem);
        }
    }
}