using System;
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
        internal static WorkflowAction Reschedule(WorkflowItem workflowItem)
        {
            return new GenericWorkflowAction(workflowItem.GetDecision());
        }
        private sealed class GenericWorkflowAction : WorkflowAction
        {
            private readonly WorkflowDecision _workflowDecision;
            public GenericWorkflowAction(WorkflowDecision workflowDecision)
            {
                _workflowDecision = workflowDecision;
            }
            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return new[] {_workflowDecision};
            }
            private bool Equals(GenericWorkflowAction other)
            {
                return _workflowDecision.Equals(other._workflowDecision);
            }
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((GenericWorkflowAction)obj);
            }
            public override int GetHashCode()
            {
                return _workflowDecision.GetHashCode();
            }
        }
        private sealed class RescheduleWorkflowAction : WorkflowAction
        {
            private readonly WorkflowItem _workflowItem;
            private readonly WorkflowAction _workflowAction;
            public RescheduleWorkflowAction(WorkflowItem workflowItem)
            {
                _workflowItem = workflowItem;
                _workflowAction = new GenericWorkflowAction(_workflowItem.GetDecision());
            }

            public WorkflowAction After(TimeSpan afterTimeout)
            {
                return  new GenericWorkflowAction(_workflowItem.GetRescheduleDecision(afterTimeout));
            }

            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return _workflowAction.GetDecisions();
            }
        }
    }
}