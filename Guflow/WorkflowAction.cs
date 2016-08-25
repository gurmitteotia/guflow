﻿using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public abstract class WorkflowAction
    {
        private static readonly WorkflowAction _emptyWorkflowAction = new EmptyWorkflowAction();
        internal abstract IEnumerable<WorkflowDecision> GetDecisions();
        internal virtual bool AllowSchedulingOfChildWorkflowItem()
        {
            return false;
        }

        public static WorkflowAction operator +(WorkflowAction left, WorkflowAction right)
        {
            return new CompositeWorkflowAction(left,right);
        }
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
        internal static ScheduleWorkflowItemAction Schedule(WorkflowItem workflowItem)
        {
            return new ScheduleWorkflowItemAction(workflowItem);
        }
        internal static WorkflowAction ContinueWorkflow(WorkflowItem workflowItem)
        {
            return new ContinueWorkflowAction(workflowItem);
        }
        internal static WorkflowAction StartWorkflow(IWorkflowItems workflowItems)
        {
            return new StartWorkflowAction(workflowItems);
        }
        internal static WorkflowAction Ignore { get{return _emptyWorkflowAction;} }
        internal static WorkflowAction Cancel(WorkflowItem workflowItem)
        {
            return new GenericWorkflowAction(workflowItem.GetCancelDecision());
        }
        internal static WorkflowAction Signal(string signalName, string input,string workflowId, string runId)
        {
            return new GenericWorkflowAction(new SignalWorkflowDecision(signalName,input,workflowId,runId));
        }
        private class EmptyWorkflowAction : WorkflowAction
        {
            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return new[] {WorkflowDecision.Empty};
            }
        }
        private class StartWorkflowAction : WorkflowAction
        {
            private const string _defaultCompleteResult = "Workflow is completed as no schedulable item is found.";
            private readonly IWorkflowItems _workflowItems;

            public StartWorkflowAction(IWorkflowItems workflowItems)
            {
                _workflowItems = workflowItems;
            }
            public override bool Equals(object other)
            {
                var otherAction = other as StartWorkflowAction;
                if (otherAction == null)
                    return false;
                return _workflowItems.Equals(otherAction._workflowItems);
            }
            public override int GetHashCode()
            {
                return _workflowItems.GetHashCode();
            }
            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                var startupWorkflowItems = _workflowItems.GetStartupWorkflowItems();

                if (!startupWorkflowItems.Any())
                    return new[] { new CompleteWorkflowDecision(_defaultCompleteResult) };

                return startupWorkflowItems.Select(s => s.GetScheduleDecision());
            }
        }

        private class CompositeWorkflowAction : WorkflowAction
        {
            private readonly WorkflowAction _left;
            private readonly WorkflowAction _right;
            public CompositeWorkflowAction(WorkflowAction left, WorkflowAction right)
            {
                _left = left;
                _right = right;
            }
            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return _left.GetDecisions().Concat(_right.GetDecisions());
            }
        }
    }
}