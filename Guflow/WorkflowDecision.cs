using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal abstract class WorkflowDecision
    {
        private readonly bool _canCloseWorkflow;
        protected readonly bool Proposal;
        internal abstract Decision Decision();
        internal static readonly WorkflowDecision Empty = new EmptyWorkflowDecision();
       
        protected WorkflowDecision(bool canCloseWorkflow, bool proposal=false)
        {
            _canCloseWorkflow = canCloseWorkflow;
            Proposal = proposal;
        }

        private sealed class EmptyWorkflowDecision : WorkflowDecision
        {
            public EmptyWorkflowDecision(): base(false)
            {
            }
            internal override Decision Decision()
            {
                throw new NotSupportedException();
            }
        }
        internal  bool IsIncompaitbleWith(IEnumerable<WorkflowDecision> workflowDecisions)
        {
            if (Proposal)
                return workflowDecisions.Any(d => !d._canCloseWorkflow);
            if (!_canCloseWorkflow)
                return workflowDecisions.Any(w => w._canCloseWorkflow && !w.Proposal);
            return false;
        }
    }
}
