using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    internal static class WorkflowDecisionExtension
    {
        public static IEnumerable<WorkflowDecision> GenerateFinalDecisionsFor(this IEnumerable<WorkflowClosingDecision> workflowClosingDecisions, IWorkflowClosingActions workflowClosingActions)
        {
            var finalClosingDecision = workflowClosingDecisions.OrderByDescending(d => d.Priority).First();
            var finalWorkflowAction = finalClosingDecision.ProvideFinalActionFrom(workflowClosingActions);
            return finalWorkflowAction==null? Enumerable.Empty<WorkflowDecision>(): finalWorkflowAction.GetDecisions();
        } 
    }
}