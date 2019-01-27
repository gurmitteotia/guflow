// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class CompositeWorkflowAction : WorkflowAction
    {
        private readonly WorkflowAction _left;
        private readonly WorkflowAction _right;
        public CompositeWorkflowAction(WorkflowAction left, WorkflowAction right)
        {
            _left = left;
            _right = right;
        }
        internal override bool ReadyToScheduleChildren => _left.ReadyToScheduleChildren || _right.ReadyToScheduleChildren;

        internal override bool CanScheduleAny(IEnumerable<WorkflowItem> workflowItems)
        {
            return _left.CanScheduleAny(workflowItems) || _right.CanScheduleAny(workflowItems);
        }

        internal override IEnumerable<WorkflowDecision> Decisions()
        {
            return _left.Decisions().Concat(_right.Decisions());
        }

        internal override IEnumerable<WaitForSignalsEvent> WaitForSignalsEvent()
        {
            return _left.WaitForSignalsEvent().Concat(_right.WaitForSignalsEvent());
        }

        internal override WorkflowAction TriggeredAction(WorkflowItem item)
        {
            return _left.TriggeredAction(item) + _right.TriggeredAction(item);
        }

        internal override WorkflowAction WithTriggeredItem(WorkflowItem item)
        {
            return _left.WithTriggeredItem(item) + _right.WithTriggeredItem(item);
        }
    }
}