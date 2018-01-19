// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    internal abstract class WorkflowClosingDecision : WorkflowDecision
    {
        internal int Priority { get; set; }
        protected WorkflowClosingDecision(bool proposal = false) : base(true, proposal)
        {
        }
        internal abstract WorkflowAction ProvideFinalActionFrom(IWorkflowClosingActions workflowClosingActions);

        internal abstract void Raise(PostExecutionEvents postExecutionEvents);
    }
}