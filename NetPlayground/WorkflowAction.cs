using System.Collections.Generic;

namespace NetPlayground
{
    public abstract class WorkflowAction
    {
        public abstract IEnumerable<WorkflowDecision> GetDecisions();
    }
}