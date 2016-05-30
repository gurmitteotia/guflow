using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class WorkflowDecision
    {
       public abstract Decision Decision();

        public static readonly WorkflowDecision Empty = new EmptyWorkflowDecision();

        private class EmptyWorkflowDecision : WorkflowDecision
        {
            public override Decision Decision()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
