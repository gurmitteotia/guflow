using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    internal class WorkflowEventsExecution : IDisposable
    {
        private readonly Workflow _workflow;
        private readonly IWorkflowHistoryEvents _historyEvents;

        public WorkflowEventsExecution(Workflow workflow, IWorkflowHistoryEvents historyEvents)
        {
            _workflow = workflow;
            _historyEvents = historyEvents;
        }

        public IEnumerable<WorkflowDecision> Execute()
        {
            var workflowDecisions = _historyEvents.InterpretNewEventsFor(_workflow);
            return FilterOutIncompatibleDecisions(workflowDecisions).Where(d => d != WorkflowDecision.Empty);
        }
        private IEnumerable<WorkflowDecision> FilterOutIncompatibleDecisions(IEnumerable<WorkflowDecision> workflowDecisions)
        {
            var compatibleWorkflows = workflowDecisions.Where(d => !d.IsIncompaitbleWith(workflowDecisions.Where(f => !f.Equals(d)))).ToArray();

            var workflowClosingDecisions = compatibleWorkflows.OfType<WorkflowClosingDecision>();
            if (workflowClosingDecisions.Any())
                return workflowClosingDecisions.GenerateFinalDecisionsFor(_workflow);

            return compatibleWorkflows;
        }

        public void Dispose()
        {
            _workflow.FinishExecution();
        }
    }
}