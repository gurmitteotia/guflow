using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public abstract class WorkflowAction
    {
        public IEnumerable<Decision> GetDecisions()
        {
            return GetDecision().Decisions();
        }
        protected abstract WorkflowDecision GetDecision();
    }
}