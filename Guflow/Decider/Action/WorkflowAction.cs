// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent instructions to Amazon SWF which are send in response to an event.
    /// </summary>
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
        internal virtual IEnumerable<WorkflowDecision> Decisions()
        {
            return _workflowDecisions;
        }
        internal virtual bool ReadyToScheduleChildren => false;

        internal virtual WorkflowAction TriggeredAction(WorkflowItem item) => this;

        internal virtual WorkflowAction WithTriggeredItem(WorkflowItem item) => this;

        internal virtual bool CanScheduleAny(IEnumerable<WorkflowItem> workflowItems)
        {
             return Decisions().Any(d => workflowItems.Any(d.IsFor));
        }

        internal virtual IEnumerable<WaitForSignalsEvent> WaitForSignalsEvent() => Enumerable.Empty<WaitForSignalsEvent>();
        
        /// <summary>
        /// Combine two workflow actions togather. Useful if multiple workflow actions needs to be returned in response of an event.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static WorkflowAction operator+(WorkflowAction left, WorkflowAction right)
        {
            Ensure.NotNull(left,nameof(left));
            Ensure.NotNull(right, nameof(right));
            return new CompositeWorkflowAction(left,right);
        }

        /// <summary>
        /// Combine two workflow actions togather. Useful if multiple workflow actions needs to be returned in response of an event.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public WorkflowAction And(WorkflowAction other)
        {
            return this + other;
        }
        internal static readonly WorkflowAction Empty = new WorkflowAction(Enumerable.Empty<WorkflowDecision>());

        internal static WorkflowAction Custom(params WorkflowDecision[] workflowDecisions)
        {
            return new WorkflowAction(workflowDecisions);
        }
        internal static WorkflowAction FailWorkflow(string reason, object detail)
        {
            return new WorkflowAction(new FailWorkflowDecision(reason,detail.ToAwsString()));
        }
        internal static WorkflowAction CompleteWorkflow(object result)
        {
            return new WorkflowAction(new CompleteWorkflowDecision(result.ToAwsString()));
        }
        internal static WorkflowAction CancelWorkflow(object detail)
        {
            return new WorkflowAction(new CancelWorkflowDecision(detail.ToAwsString()));
        }
        internal static ScheduleWorkflowItemAction Schedule(WorkflowItem workflowItem)
        {
            return ScheduleWorkflowItemAction.ScheduleByConsideringWhen(workflowItem);
        }
        internal static JumpWorkflowAction JumpTo(WorkflowItem triggerItem, WorkflowItem jumpItem)
        {
            return new JumpWorkflowAction(triggerItem, jumpItem);
        }
        internal static WorkflowAction ContinueWorkflow(WorkflowItem workflowItem)
        {
            return new ContinueWorkflowAction(workflowItem);
        }
        internal static WorkflowAction StartWorkflow(WorkflowItems workflowItems)
        {
            return new StartWorkflowAction(workflowItems);
        }

        internal static IgnoreWorkflowAction Ignore(WorkflowItem triggerItem)=> new IgnoreWorkflowAction(triggerItem);

        internal static WorkflowAction Cancel(WorkflowItem workflowItem)
        {
            return new WorkflowAction(workflowItem.CancelDecisions());
        }
        internal static WorkflowAction Cancel(IEnumerable<WorkflowItem> workflowItems)
        {
            return new WorkflowAction(workflowItems.SelectMany(w=>w.CancelDecisions()));
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
        internal static RestartWorkflowAction RestartWorkflowWithDefaultProperties()
            => new RestartWorkflowAction();
       
    }
}