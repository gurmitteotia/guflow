using System.Collections.Generic;

namespace NetPlayground
{
    public abstract class WorkflowEvent
    {
        public abstract WorkflowAction Interpret(IWorkflow workflow);

        public abstract IWorkflowContext WorkflowContext { get; }
    }
}