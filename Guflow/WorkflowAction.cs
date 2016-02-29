using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public abstract class WorkflowAction
    {
        private static readonly WorkflowAction _emptyWorkflowAction = new EmptyWorkflowAction();
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
        internal static WorkflowAction ContinueWorkflow(WorkflowItem workflowItem,IWorkflowContext workflowContext)
        {
            return new ContinueWorkflowAction(workflowItem,workflowContext);
        }
        internal static WorkflowAction StartWorkflow(IWorkflowItems workflowItems)
        {
            return new StartWorkflowAction(workflowItems);
        }
        internal static WorkflowAction Ignore { get{return _emptyWorkflowAction;} }

        private class EmptyWorkflowAction : WorkflowAction
        {
            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return Enumerable.Empty<WorkflowDecision>();
            }
        }
    }
}