using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public abstract class WorkflowAction
    {
        internal abstract IEnumerable<WorkflowDecision> GetDecisions();
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
        internal static readonly WorkflowAction Empty = new EmptyWorkflowAction();
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
        private sealed class EmptyWorkflowAction : WorkflowAction
        {
            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return Enumerable.Empty<WorkflowDecision>();
            }
        }
    }
}