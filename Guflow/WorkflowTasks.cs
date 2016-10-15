using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class WorkflowTasks
    {
        private readonly DecisionTask _decisionTask;
        private readonly IWorkflowClient _workflowClient;
        private readonly Action<HostedWorkflows> _actionToExecute;  
        private WorkflowTasks(DecisionTask decisionTask, IWorkflowClient workflowClient)
        {
            _decisionTask = decisionTask;
            _workflowClient = workflowClient;
            _actionToExecute = ExecuteTasks;
        }

        private WorkflowTasks()
        {
            _actionToExecute = w => { };
        }

        public static readonly WorkflowTasks Empty = new WorkflowTasks();

        public static WorkflowTasks CreateFor(DecisionTask decisionTask, IWorkflowClient workflowClient)
        {
            return new WorkflowTasks(decisionTask,workflowClient);
        }

        public void ExecuteFor(HostedWorkflows hostedWorkflows)
        {
            _actionToExecute(hostedWorkflows);
        }

        private void ExecuteTasks(HostedWorkflows hostedWorkflows)
        {
            var workflowType = _decisionTask.WorkflowType;
            var workflow = hostedWorkflows.FindBy(workflowType.Name, workflowType.Version);
            var historyEvents = new WorkflowHistoryEvents(_decisionTask.Events, _decisionTask.StartedEventId, _decisionTask.PreviousStartedEventId);

            using (var execution = workflow.NewExecutionFor(historyEvents))
            {
                var decisions = execution.Execute();
                var swfDecisions = decisions.Select(s => s.Decision());
                _workflowClient.RespondWithDecisions(_decisionTask.TaskToken, swfDecisions);
            }
        }
    }
}