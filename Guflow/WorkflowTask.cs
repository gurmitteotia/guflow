using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class WorkflowTask
    {
        private readonly DecisionTask _decisionTask;
        private readonly IAmazonSimpleWorkflow _client;
        private readonly Func<HostedWorkflows,CancellationToken,Task> _actionToExecute;
        private WorkflowTask(DecisionTask decisionTask, IAmazonSimpleWorkflow client)
        {
            _decisionTask = decisionTask;
            _client = client;
            _actionToExecute = ExecuteTasks;
        }

        private WorkflowTask()
        {
            _actionToExecute =  async  (w,c) => { await Task.Yield(); };
        }

        public static readonly WorkflowTask Empty = new WorkflowTask();

        public static WorkflowTask CreateFor(DecisionTask decisionTask, Domain domain)
        {
            return new WorkflowTask(decisionTask, domain.Client);
        }

        public async Task ExecuteForAsync(HostedWorkflows hostedWorkflows, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _actionToExecute(hostedWorkflows, cancellationToken);
        }

        private async Task ExecuteTasks(HostedWorkflows hostedWorkflows, CancellationToken cancellationToken)
        {
            var workflowType = _decisionTask.WorkflowType;
            var workflow = hostedWorkflows.FindBy(workflowType.Name, workflowType.Version);
            var historyEvents = new WorkflowHistoryEvents(_decisionTask.Events, _decisionTask.StartedEventId, _decisionTask.PreviousStartedEventId);
            using (var execution = workflow.NewExecutionFor(historyEvents))
            {
                var decisions = execution.Execute().ToArray();
                await SendDecisionsAsync(decisions, cancellationToken);
                RaisePostExecutionEvents(decisions, workflow);
            }
        }

        private async Task SendDecisionsAsync(IEnumerable<WorkflowDecision> decisions, CancellationToken cancellationToken)
        {
            var decisionsResponse = ResponseFrom(decisions);
            await _client.RespondDecisionTaskCompletedAsync(decisionsResponse, cancellationToken);
        }

        private void RaisePostExecutionEvents(IEnumerable<WorkflowDecision> workflowDecisions, Workflow workflow)
        {
            var postExecutionEvents = new PostExecutionEvents(workflow, _decisionTask.WorkflowExecution.WorkflowId, _decisionTask.WorkflowExecution.RunId);
            var workflowClosingDecisions = workflowDecisions.OfType<WorkflowClosingDecision>().ToList();
            workflowClosingDecisions.ForEach(d=>d.Raise(postExecutionEvents));
        }

        private RespondDecisionTaskCompletedRequest ResponseFrom(IEnumerable<WorkflowDecision> decisions)
        {
            return new RespondDecisionTaskCompletedRequest
            {
                TaskToken = _decisionTask.TaskToken,
                Decisions = decisions.Select(s => s.Decision()).ToList()
            };
        }
    }
}