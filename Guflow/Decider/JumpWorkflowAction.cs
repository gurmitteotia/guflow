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
        private WorkflowAction _triggeredAction = Empty;
        private readonly ScheduleWorkflowItemAction _scheduleAction;
        internal JumpWorkflowAction(WorkflowItem jumpToItem)
        {
            _scheduleAction = new ScheduleWorkflowItemAction(jumpToItem);
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _scheduleAction.GetDecisions().Concat(_triggeredAction.GetDecisions());
        }

        /// <summary>
        /// Provide a custom trigger action for first joint items when jumping down the executing branch.
        /// </summary>
        /// <param name="triggerAction"></param>
        /// <returns></returns>
        internal JumpWorkflowAction WithTriggerAction(WorkflowAction triggerAction)
        {
            _triggeredAction = triggerAction;
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
        private bool Equals(JumpWorkflowAction other)
        {
            return _scheduleAction.Equals(other._scheduleAction);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JumpWorkflowAction)obj);
        }

        public override int GetHashCode()
        {
            return _scheduleAction.GetHashCode();
        }
    }
  }