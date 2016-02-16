﻿using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class CompleteWorkflowDecision : WorkflowDecision
    {
        private readonly string _result;
        public CompleteWorkflowDecision(string result)
        {
            _result = result;
        }

        public override bool Equals(object other)
        {
            var otherDecision = other as CompleteWorkflowDecision;
            if (otherDecision == null)
                return false;
            return string.Equals(_result, otherDecision._result);
        }

        public override int GetHashCode()
        {
            return _result == null ? GetType().GetHashCode() : _result.GetHashCode();
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