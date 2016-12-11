using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class WorkflowTask
    {
        private readonly DecisionTask _decisionTask;
        private readonly Domain _domain;
        private readonly Func<HostedWorkflows,CancellationToken,Task> _actionToExecute;
        private WorkflowTask(DecisionTask decisionTask, Domain domain)
        {
            _decisionTask = decisionTask;
            _domain = domain;
            _actionToExecute = ExecuteTasks;
        }

        private WorkflowTask()
        {
            _actionToExecute =  async  (w,c) => { await Task.Yield(); };
        }

        public static readonly WorkflowTask Empty = new WorkflowTask();

        public static WorkflowTask CreateFor(DecisionTask decisionTask, Domain domain)
        {
            return new WorkflowTask(decisionTask, domain);
        }

        public async Task ExecuteFor(HostedWorkflows hostedWorkflows, CancellationToken cancellationToken = default(CancellationToken))
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
                await SendDecisionsAsync(decisions, workflow, cancellationToken);
            }
        }

        private async Task SendDecisionsAsync(IEnumerable<WorkflowDecision> decisions, IPostExecutionEvents postExecutionEvents, CancellationToken cancellationToken)
        {
            if (this == Empty)
                return;

            var request = CreateResponseRequest(decisions);
            await _domain.RespondDecisionTaskCompletedAsync(request, cancellationToken);
            var closingDecisions = decisions.OfType<WorkflowClosingDecision>().ToList();
            closingDecisions.ForEach(d=>d.Raise(postExecutionEvents));
        }

        private RespondDecisionTaskCompletedRequest CreateResponseRequest(IEnumerable<WorkflowDecision> decisions)
        {
            return new RespondDecisionTaskCompletedRequest
            {
                TaskToken = _decisionTask.TaskToken,
                Decisions = decisions.Select(s => s.Decision()).ToList()
            };
        }
    }
}