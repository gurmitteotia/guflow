using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class WorkflowTask
    {
        private readonly DecisionTask _decisionTask;
        private readonly Func<HostedWorkflows,IAmazonSimpleWorkflow,Task> _actionToExecute;  
        private WorkflowTask(DecisionTask decisionTask)
        {
            _decisionTask = decisionTask;
            _actionToExecute = ExecuteTasks;
        }

        private WorkflowTask()
        {
            _actionToExecute = (w,c) => Task.FromResult(0);
        }

        public static readonly WorkflowTask Empty = new WorkflowTask();

        public static WorkflowTask CreateFor(DecisionTask decisionTask)
        {
            return new WorkflowTask(decisionTask);
        }

        public void ExecuteFor(HostedWorkflows hostedWorkflows, IAmazonSimpleWorkflow amazonSimpleWorkflow)
        {
            _actionToExecute(hostedWorkflows, amazonSimpleWorkflow);
        }

        private async Task ExecuteTasks(HostedWorkflows hostedWorkflows, IAmazonSimpleWorkflow amazonSimpleWorkflow)
        {
            var workflowType = _decisionTask.WorkflowType;
            var workflow = hostedWorkflows.FindBy(workflowType.Name, workflowType.Version);
            var historyEvents = new WorkflowHistoryEvents(_decisionTask.Events, _decisionTask.StartedEventId, _decisionTask.PreviousStartedEventId);

            using (var execution = workflow.NewExecutionFor(historyEvents))
            {
                var decisions = execution.Execute();
                var swfDecisions = decisions.Select(s => s.Decision());
                var responseRequest = new RespondDecisionTaskCompletedRequest()
                {
                    TaskToken = _decisionTask.TaskToken,
                    Decisions = swfDecisions.ToList()
                };
                await amazonSimpleWorkflow.RespondDecisionTaskCompletedAsync(responseRequest);
            }
        }
    }
}