// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class StartWorkflowAction : WorkflowAction
    {
        private const string DefaultCompleteResult = "Workflow is completed because no schedulable item was found.";
        private readonly WorkflowItems _workflowItems;

        public StartWorkflowAction(WorkflowItems workflowItems)
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
        internal override IEnumerable<WorkflowDecision> Decisions(IWorkflow workflow)
        {
            var startupWorkflowItems = _workflowItems.StartupItems().ToArray();

            if (!startupWorkflowItems.Any())
                return new[] { new CompleteWorkflowDecision(DefaultCompleteResult) };

            return startupWorkflowItems.SelectMany(s => s.ScheduleDecisions()).ToArray();
        }
    }
}