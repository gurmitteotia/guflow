using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class WorkflowCompleteDecision : WorkflowDecision
    {
        private readonly string _result;
        public WorkflowCompleteDecision(string result)
        {
            _result = result;
        }

        public override IEnumerable<Decision> Decisions()
        {
            var decision = new Decision()
            {
                DecisionType = DecisionType.CompleteWorkflowExecution,
                CompleteWorkflowExecutionDecisionAttributes = new CompleteWorkflowExecutionDecisionAttributes()
                {
                    Result = _result
                }
            };

            return new[] {decision};
        }
    }
}
