﻿using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public class WorkflowAction
    {
        private readonly IEnumerable<WorkflowDecision> _workflowDecisions;

        private WorkflowAction(IEnumerable<WorkflowDecision> workflowDecisions)
        {
            _workflowDecisions = workflowDecisions;
        }

        private WorkflowAction(WorkflowDecision workflowDecision)
            :this(new []{workflowDecision})
        {
        }
        protected WorkflowAction():this(Enumerable.Empty<WorkflowDecision>())
        {
        }
        internal virtual IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _workflowDecisions;
        }
        internal virtual bool ReadyToScheduleChildren => false;

        internal virtual bool CanScheduleAny(IEnumerable<WorkflowItem> workflowItems)
        {
             var decisions = GetDecisions();
             return decisions.Any(d => workflowItems.Any(d.IsFor));
        }
        
        public static WorkflowAction operator +(WorkflowAction left, WorkflowAction right)
        {
            Ensure.NotNull(left,nameof(left));
            Ensure.NotNull(right, nameof(right));
            return new CompositeWorkflowAction(left,right);
        }
        public WorkflowAction And(WorkflowAction other)
        {
            return this + other;
        }
        internal static readonly WorkflowAction Empty = new WorkflowAction(Enumerable.Empty<WorkflowDecision>());

        internal static WorkflowAction Custom(params WorkflowDecision[] workflowDecisions)
        {
            return new WorkflowAction(workflowDecisions);
        }
        internal static WorkflowAction FailWorkflow(string reason, string detail)
        {
            return new WorkflowAction(new FailWorkflowDecision(reason,detail));
        }
        internal static WorkflowAction CompleteWorkflow(string result)
        {
            return new WorkflowAction(new CompleteWorkflowDecision(result));
        }
        internal static WorkflowAction CancelWorkflow(string detail)
        {
            return new WorkflowAction(new CancelWorkflowDecision(detail));
        }
        internal static ScheduleWorkflowItemAction Schedule(WorkflowItem workflowItem)
        {
            return new ScheduleWorkflowItemAction(workflowItem);
        }
        internal static JumpWorkflowAction JumpTo(WorkflowItem workflowItem)
        {
            return new JumpWorkflowAction(workflowItem);
        }
        internal static WorkflowAction ContinueWorkflow(WorkflowItem workflowItem)
        {
            return new ContinueWorkflowAction(workflowItem);
        }
        internal static WorkflowAction StartWorkflow(WorkflowItems workflowItems)
        {
            return new StartWorkflowAction(workflowItems);
        }

        internal static WorkflowAction Ignore(bool keepBranchActive)
        {
            return new IgnoreWorkflowAction(keepBranchActive);
        }
        internal static WorkflowAction Cancel(WorkflowItem workflowItem)
        {
            return new WorkflowAction(workflowItem.GetCancelDecision());
        }
        internal static WorkflowAction Cancel(IEnumerable<WorkflowItem> workflowItems)
        {
            return new WorkflowAction(workflowItems.Select(w=>w.GetCancelDecision()));
        }
        internal static WorkflowAction Signal(string signalName, string input,string workflowId, string runId)
        {
            return new WorkflowAction(new SignalWorkflowDecision(signalName,input,workflowId,runId));
        }
        internal static WorkflowAction RecordMarker(string markerName, string details)
        {
            return new WorkflowAction(new RecordMarkerWorkflowDecision(markerName,details));
        }
        internal static WorkflowAction CancelWorkflowRequest(string workflowId, string runId)
        {
            return new WorkflowAction(new CancelRequestWorkflowDecision(workflowId, runId));
        }
        internal static RestartWorkflowAction RestartWorkflow(IWorkflowHistoryEvents workflowHistoryEvents)
        {
            var workflowStartedEvent = workflowHistoryEvents.WorkflowStartedEvent();
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
    }
}