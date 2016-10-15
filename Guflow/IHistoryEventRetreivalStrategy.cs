using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public interface IHistoryEventRetreivalStrategy
    {
        DecisionTask RetreiveEvents(DecisionTask retreivedTask, IWorkflowClient workflowClient);
    }
}