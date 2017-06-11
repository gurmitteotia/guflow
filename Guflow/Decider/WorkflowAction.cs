﻿using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public abstract class WorkflowAction
    {
        internal abstract IEnumerable<WorkflowDecision> GetDecisions();
        internal virtual bool ReadyToScheduleChildren
        {
            get { return false; }
        }

        internal virtual bool CanKeepBranchActive(IEnumerable<WorkflowItem> branchWorkflowItems)
        {
             var decisions = GetDecisions();
             return decisions.Any(d => branchWorkflowItems.Any(d.IsFor));
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

        internal static WorkflowAction Ignore(bool keepBranchActive)
        {
            return new IgnoreWorkflowAction(keepBranchActive);
        }

        internal static WorkflowAction Cancel(WorkflowItem workflowItem)
        {
            return new GenericWorkflowAction(workflowItem.GetCancelDecision());
        }
        internal static WorkflowAction Cancel(IEnumerable<WorkflowItem> workflowItems)
        {
            return new CancelItemsWorkflowAction(workflowItems);
        }
        internal static WorkflowAction Signal(string signalName, string input,string workflowId, string runId)
        {
            return new GenericWorkflowAction(new SignalWorkflowDecision(signalName,input,workflowId,runId));
        }
        internal static WorkflowAction RecordMarker(string markerName, string details)
        {
            return new GenericWorkflowAction(new RecordMarkerWorkflowDecision(markerName,details));
        }
        internal static WorkflowAction CancelWorkflowRequest(string workflowId, string runId)
        {
            return new GenericWorkflowAction(new CancelRequestWorkflowDecision(workflowId, runId));
        }
        internal static RestartWorkflowAction RestartWorkflow(IWorkflowEvents workflowEvents)
        {
            var workflowStartedEvent = workflowEvents.WorkflowStartedEvent();
            var restartWorkflowAction = new RestartWorkflowAction()
            {
                Input = workflowStartedEvent.Input,
                TaskList = workflowStartedEvent.TaskList,
                ExecutionStartToCloseTimeout = workflowStartedEvent.ExecutionStartToCloseTimeout,
                TaskPriority = workflowStartedEvent.TaskPriority,
                TaskStartToCloseTimeout = workflowStartedEvent.TaskStartToCloseTimeout,
                ChildPolicy = workflowStartedEvent.ChildPolicy,
            };
            workflowStartedEvent.TagList.ToList().ForEach(tag=>restartWorkflowAction.AddTag(tag));
            return restartWorkflowAction;
        }

        private sealed class IgnoreWorkflowAction : WorkflowAction
        {
            private readonly bool _keepBranchActive;

            public IgnoreWorkflowAction(bool keepBranchActive)
            {
                _keepBranchActive = keepBranchActive;
            }

            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return Enumerable.Empty<WorkflowDecision>();
            }

            internal override bool CanKeepBranchActive(IEnumerable<WorkflowItem> branchWorkflowItems)
            {
                return _keepBranchActive;
            }

            internal override bool ReadyToScheduleChildren
            {
                get { return !_keepBranchActive; }
            }

            private bool Equals(IgnoreWorkflowAction other)
            {
                return _keepBranchActive == other._keepBranchActive;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((IgnoreWorkflowAction)obj);
            }
            public override int GetHashCode()
            {
                return _keepBranchActive.GetHashCode();
            }

        }

        private class StartWorkflowAction : WorkflowAction
        {
            private const string _defaultCompleteResult = "Workflow is completed because no schedulable item was found.";
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

                return startupWorkflowItems.SelectMany(s => s.GetContinuedDecisions());
            }
        }

        private class CancelItemsWorkflowAction : WorkflowAction
        {
            private readonly IEnumerable<WorkflowItem> _workflowItems;

            public CancelItemsWorkflowAction(IEnumerable<WorkflowItem> workflowItems)
            {
                _workflowItems = workflowItems;
            }

            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return _workflowItems.Select(w => w.GetCancelDecision());
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
            internal override bool ReadyToScheduleChildren
            {
                get { return  _left.ReadyToScheduleChildren || _right.ReadyToScheduleChildren; }
            }

            internal override bool CanKeepBranchActive(IEnumerable<WorkflowItem> branchWorkflowItems)
            {
                return _left.CanKeepBranchActive(branchWorkflowItems) || _right.CanKeepBranchActive(branchWorkflowItems);
            }

            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return _left.GetDecisions().Concat(_right.GetDecisions());
            }
        }

    }
}