// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class WorkflowEventsExecution : IDisposable
    {
        private readonly Workflow _workflow;
        private readonly IWorkflowHistoryEvents _workflowHistoryEvents;

        public WorkflowEventsExecution(Workflow workflow, IWorkflowHistoryEvents workflowHistoryEvents)
        {
            _workflow = workflow;
            _workflowHistoryEvents = workflowHistoryEvents;
        }

        public IEnumerable<WorkflowDecision> Execute()
        {
            _workflow.BeforeExecution();
            try
            {
                var workflowDecisions = _workflowHistoryEvents.InterpretNewEvents(_workflow).ToArray();
                return FilterOutIncompatibleDecisions(workflowDecisions).Where(d => d != WorkflowDecision.Empty);
            }
            finally
            {
                _workflow.AfterExecution();
            }
        }
        private IEnumerable<WorkflowDecision> FilterOutIncompatibleDecisions(WorkflowDecision[] workflowDecisions)
        {
            var compatibleWorkflows = workflowDecisions.Where(d => !d.IsIncompaitbleWith(workflowDecisions.Where(f => !f.Equals(d)))).ToArray();

            var closingDecisions = compatibleWorkflows.OfType<WorkflowClosingDecision>().ToArray();
            if (closingDecisions.Any())
                return closingDecisions.GenerateFinalDecisionsFor(_workflow);

            return compatibleWorkflows;
        }

        public void Dispose()
        {
            _workflow.FinishExecution();
        }
    }
}