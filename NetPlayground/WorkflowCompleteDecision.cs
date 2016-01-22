using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class WorkflowCompleteDecision : WorkflowDecision
    {
        public override IEnumerable<Decision> Decisions()
        {
            var decision = new Decision()
            {
                DecisionType = DecisionType.CompleteWorkflowExecution,
                CompleteWorkflowExecutionDecisionAttributes = new CompleteWorkflowExecutionDecisionAttributes()
                {
                    Result = "SomeResult"
                }
            };

            return new[] {decision};
        }
    }
}
