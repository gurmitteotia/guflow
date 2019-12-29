// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    /// <summary>
    /// Represents a workflow action to jump to child item of workflow.
    /// </summary>
    public sealed class JumpWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _jumpToItem;
        private WorkflowAction _triggeredAction;
        private readonly ScheduleWorkflowItemAction _scheduleAction;
        private WorkflowItem _triggerItem;
        

        internal JumpWorkflowAction(WorkflowItem triggerItem, WorkflowItem jumpToItem)
        {
            _triggerItem = triggerItem;
            _jumpToItem = jumpToItem;
            _scheduleAction = ScheduleWorkflowItemAction.ScheduleByIgnoringWhen(jumpToItem);
            _triggeredAction = Default();
        }
        internal override IEnumerable<WorkflowDecision> Decisions(IWorkflow workflow)
        {
            return _scheduleAction.Decisions(workflow).Concat(_triggeredAction.Decisions(workflow));
        }

        internal override WorkflowAction WithTriggeredItem(WorkflowItem item)
        {
            _triggerItem = item;
            _triggeredAction.WithTriggeredItem(_triggerItem);
            return this;
        }
        /// <summary>
        /// Do not trigger the scheduling of first joint items after source item. By default jumping download the executing branch will trigger
        /// the scheduling of first joint item.
        /// </summary>
        /// <returns></returns>
        public WorkflowAction WithoutTrigger()
        {
            _triggeredAction = Empty;
            return this;
        }

        /// <summary>
        /// Jump after a timeout, instead of immediately.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public WorkflowAction After(TimeSpan timeout)
        {
            return _scheduleAction.After(timeout);
        }
        private WorkflowAction Default()
        {
            return _triggerItem != null ? new TriggerActions(_triggerItem).FirstJoint(_jumpToItem) : Empty;
        }
    }
  }