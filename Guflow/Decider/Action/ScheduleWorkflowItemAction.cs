// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    /// <summary>
    /// Cause a workflow item to schedule.
    /// </summary>
    public sealed class ScheduleWorkflowItemAction : WorkflowAction
    {
        private readonly WorkflowItem _workflowItem;
        private WorkflowAction _scheduleWorkflowAction;

        private ScheduleWorkflowItemAction(WorkflowItem workflowItem, WorkflowAction workflowAction)
        {
            _workflowItem = workflowItem;
            _scheduleWorkflowAction = workflowAction;
        }

        internal static ScheduleWorkflowItemAction ScheduleByConsideringWhen(WorkflowItem workflowItem)
        {
            return new ScheduleWorkflowItemAction(workflowItem, Custom(workflowItem.ScheduleDecisions().ToArray()));
        }

        internal static ScheduleWorkflowItemAction ScheduleByIgnoringWhen(WorkflowItem workflowItem)
        {
            return new ScheduleWorkflowItemAction(workflowItem, Custom(workflowItem.ScheduleDecisionsByIgnoringWhen().ToArray()));
        }

        /// <summary>
        /// Cause the item to schedule after a given timeout.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public ScheduleWorkflowItemAction After(TimeSpan timeout)
        {
           _scheduleWorkflowAction = Custom(_workflowItem.RescheduleDecisions(timeout).ToArray());
            return this;
        }
        /// <summary>
        /// Limit the scheduling. Once the limit is reached, Guflow returns the default WorkflowAction for event.
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public WorkflowAction UpTo(uint times)
        {
            var limit = Limit.Count(times);
            if (limit.IsExceeded(_workflowItem))
                _scheduleWorkflowAction = _workflowItem.DefaultActionOnLastEvent();
            return this;
        }
        internal override IEnumerable<WorkflowDecision> Decisions(IWorkflow workflow)
        {
            return _scheduleWorkflowAction.Decisions(workflow);
        }
    }
}