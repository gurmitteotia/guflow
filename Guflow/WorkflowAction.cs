using System.Collections.Generic;

namespace Guflow
{
    public abstract class WorkflowAction
    {
        public abstract IEnumerable<WorkflowDecision> GetDecisions();
    }
}