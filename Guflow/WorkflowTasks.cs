using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class WorkflowTasks
    {
        private readonly DecisionTask _decisionTask;
        private readonly IWorkflowClient _workflowClient;

        public WorkflowTasks(DecisionTask decisionTask, IWorkflowClient workflowClient)
        {
            _decisionTask = decisionTask;
            _workflowClient = workflowClient;
        }

        public void ExecuteFor(HostedWorkflows hostedWorkflows)
        {
            var workflowType = _decisionTask.WorkflowType;
            var workflow = hostedWorkflows.FindBy(workflowType.Name, workflowType.Version);
            var historyEvents = new WorkflowHistoryEvents(_decisionTask.Events,_decisionTask.StartedEventId,_decisionTask.PreviousStartedEventId);

            using (var execution = workflow.NewExecutionFor(historyEvents))
            {
                var decisions = execution.Execute();
                var swfDecisions = decisions.Select(s => s.Decision());
                _workflowClient.RespondWithDecisions(_decisionTask.TaskToken, swfDecisions);
            }
        }
    }
}