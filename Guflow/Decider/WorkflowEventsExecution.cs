using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class WorkflowEventsExecution : IDisposable
    {
        private readonly Workflow _workflow;
        private readonly IWorkflowEvents _workflowEvents;

        public WorkflowEventsExecution(Workflow workflow, IWorkflowEvents workflowEvents)
        {
            _workflow = workflow;
            _workflowEvents = workflowEvents;
        }

        public IEnumerable<WorkflowDecision> Execute()
        {
            var workflowDecisions = _workflowEvents.InterpretNewEventsFor(_workflow);
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