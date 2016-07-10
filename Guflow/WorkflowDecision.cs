using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class WorkflowDecision
    {
        internal abstract Decision Decision();

        public static readonly WorkflowDecision Empty = new EmptyWorkflowDecision();

        private sealed class EmptyWorkflowDecision : WorkflowDecision
        {
            internal override Decision Decision()
            {
                throw new NotSupportedException();
            }
        }
        internal virtual bool IsCompaitbleWith(IEnumerable<WorkflowDecision> workflowDecisions)
        {
            return true;
        }
    }
}
