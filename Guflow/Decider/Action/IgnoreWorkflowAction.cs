// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;

namespace Guflow.Decider
{
    /// <summary>
    /// Supports ignoring the a event. i.e. takes no action.
    /// </summary>
    public sealed class IgnoreWorkflowAction : WorkflowAction
    {
        private readonly WorkflowItem _triggeringItem;
        private WorkflowAction _triggerAction = Empty;
        private bool _keepBranchActive;
        internal IgnoreWorkflowAction(WorkflowItem triggeringItem)
        {
            _triggeringItem = triggeringItem;
            _keepBranchActive = triggeringItem!=null;
        }

        /// <summary>
        /// Ignore action will make the branch inactive. While making the branch it will try to trigger the scheduling of first joint item.
        /// </summary>
        /// <returns></returns>
        public WorkflowAction MakeBranchInactive()
        {
            _keepBranchActive = false;
            if(_triggeringItem!=null)
                _triggerAction = new TriggerActions(_triggeringItem).FirstJoint();
            return this;
        }

        /// <summary>
        /// Makes the branch inactive and override the trigger workflow workflow.
        /// </summary>
        /// <param name="triggerAction"></param>
        /// <returns></returns>
        public WorkflowAction MakeBranchInactive(WorkflowAction triggerAction)
        {
            Ensure.NotNull(triggerAction, "trigerAction");
            _triggerAction = triggerAction;
            _keepBranchActive = false;
            return this;
        }

        internal override IEnumerable<WorkflowDecision> Decisions()
        {
            return _triggerAction.Decisions();
        }

        internal override bool CanScheduleAny(IEnumerable<WorkflowItem> workflowItems)
        {
            return _keepBranchActive;
        }

        internal override bool ReadyToScheduleChildren => !_keepBranchActive;

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
}