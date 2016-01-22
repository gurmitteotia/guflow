using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    internal class TestWorkflowDecision : WorkflowDecision
    {
        public override IEnumerable<Decision> Decisions()
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestWorkflowAction : WorkflowAction
    {
        protected override WorkflowDecision GetDecision()
        {
            throw new System.NotImplementedException();
        }
    }
}