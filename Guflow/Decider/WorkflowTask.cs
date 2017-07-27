using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WorkflowTask
    {
        private readonly DecisionTask _decisionTask;
        private readonly Func<HostedWorkflows,CancellationToken,Task> _actionToExecute;
        private IErrorHandler _executionErrorHandler = ErrorHandler.NotHandled;
        private static readonly WorkflowDecision[] EmptyDecisions = new WorkflowDecision[0];

        private WorkflowTask(DecisionTask decisionTask)
        {
            _decisionTask = decisionTask;
            _actionToExecute = ExecuteTasks;
        }

        private WorkflowTask()
        {
            _actionToExecute =  async  (w,c) => { await Task.Yield(); };
        }

        public static readonly WorkflowTask Empty = new WorkflowTask();

        public static WorkflowTask CreateFor(DecisionTask decisionTask, Domain domain)
        {
            return new WorkflowTask(decisionTask);
        }

        public async Task ExecuteForAsync(HostedWorkflows hostedWorkflows, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _actionToExecute(hostedWorkflows, cancellationToken);
        }
        internal void OnExecutionError(IErrorHandler errorHandler)
        {
            _executionErrorHandler = errorHandler;
        }
        private async Task ExecuteTasks(HostedWorkflows hostedWorkflows, CancellationToken cancellationToken)
        {
            var workflowType = _decisionTask.WorkflowType;
            var workflow = hostedWorkflows.FindBy(workflowType.Name, workflowType.Version);
            var historyEvents = new WorkflowHistoryHistoryEvents(_decisionTask.Events, _decisionTask.StartedEventId, _decisionTask.PreviousStartedEventId);
            using (var execution = workflow.NewExecutionFor(historyEvents))
            {
                var decisions = Perform(execution);
                await hostedWorkflows.SendDecisionsAsync(_decisionTask.TaskToken, decisions);
                RaisePostExecutionEvents(decisions, workflow);
            }
        }
        private WorkflowDecision [] Perform(WorkflowEventsExecution execution)
        {
            var retryableAction = new RetryableFunc(_executionErrorHandler);
            return retryableAction.Execute(()=>execution.Execute().ToArray(), EmptyDecisions);
        }
        private void RaisePostExecutionEvents(IEnumerable<WorkflowDecision> workflowDecisions, Workflow workflow)
        {
            var postExecutionEvents = new PostExecutionEvents(workflow, _decisionTask.WorkflowExecution.WorkflowId, _decisionTask.WorkflowExecution.RunId);
            var workflowClosingDecisions = workflowDecisions.OfType<WorkflowClosingDecision>().ToList();
            workflowClosingDecisions.ForEach(d=>d.Raise(postExecutionEvents));
        }
    }
}