// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal static class WorkflowDecisionsExtension
    {
        public static IEnumerable<WorkflowDecision> CompatibleDecisions(this IEnumerable<WorkflowDecision> decisions,
            Workflow workflow)
        {
            var compatibleWorkflows = decisions.Where(d => !d.IsIncompaitbleWith(decisions.Where(f => !f.Equals(d)))).ToArray();

            var closingDecisions = compatibleWorkflows.OfType<WorkflowClosingDecision>().ToArray();
            if (closingDecisions.Any())
                return closingDecisions.FinalDecisions(workflow);

            return compatibleWorkflows;
        }

        private static IEnumerable<WorkflowDecision> FinalDecisions(this IEnumerable<WorkflowClosingDecision> workflowClosingDecisions, Workflow workflow)
        {
            var finalClosingDecision = workflowClosingDecisions.OrderByDescending(d => d.Priority).First();
            var finalWorkflowAction = finalClosingDecision.ProvideFinalActionFrom(workflow);
            return finalWorkflowAction == null ? Enumerable.Empty<WorkflowDecision>() : finalWorkflowAction.Decisions(workflow);
        }
    }
}