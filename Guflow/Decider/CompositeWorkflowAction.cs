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

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _left.GetDecisions().Concat(_right.GetDecisions());
        }
    }
}