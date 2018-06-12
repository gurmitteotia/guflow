// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal static class WorkflowDecisionExtension
    {
        public static IEnumerable<WorkflowDecision> GenerateFinalDecisionsFor(this IEnumerable<WorkflowClosingDecision> workflowClosingDecisions, IWorkflowClosingActions workflowClosingActions)
        {
            var finalClosingDecision = workflowClosingDecisions.OrderByDescending(d => d.Priority).First();
            var finalWorkflowAction = finalClosingDecision.ProvideFinalActionFrom(workflowClosingActions);
            return finalWorkflowAction==null? Enumerable.Empty<WorkflowDecision>(): finalWorkflowAction.Decisions();
        } 
    }
}