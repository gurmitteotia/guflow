﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class CancelRequestWorkflowDecision : WorkflowDecision
    {
        private readonly string _workflowId;
        private readonly string _runId;

        public CancelRequestWorkflowDecision(string workflowId, string runId)
            : base(false)
        {
            _workflowId = workflowId;
            _runId = runId;
        }

        internal override Decision SwfDecision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.RequestCancelExternalWorkflowExecution,
                RequestCancelExternalWorkflowExecutionDecisionAttributes =
                    new RequestCancelExternalWorkflowExecutionDecisionAttributes()
                    {
                        WorkflowId = _workflowId,
                        RunId = _runId
                    }
            };
        }

        private bool Equals(CancelRequestWorkflowDecision other)
        {
            return string.Equals(_workflowId, other._workflowId) && string.Equals(_runId, other._runId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CancelRequestWorkflowDecision)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_workflowId.GetHashCode() * 397) ^ (_runId == null ? 10 : _runId.GetHashCode());
            }
        }

    }
}