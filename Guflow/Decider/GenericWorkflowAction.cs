using System.Collections.Generic;

namespace Guflow.Decider
{
    internal sealed class GenericWorkflowAction : WorkflowAction
    {
        private readonly IEnumerable<WorkflowDecision> _workflowDecisions;
        public GenericWorkflowAction(IEnumerable<WorkflowDecision> workflowDecisions)
        {
            _workflowDecisions = workflowDecisions;
        }
        public GenericWorkflowAction(WorkflowDecision workflowDecision)
            :this(new []{workflowDecision})
        {
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _workflowDecisions;
        }
    }
}