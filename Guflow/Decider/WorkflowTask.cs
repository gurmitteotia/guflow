using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WorkflowTask
    {
        private readonly DecisionTask _decisionTask;
        private readonly IAmazonSimpleWorkflow _client;
        private readonly Func<HostedWorkflows,CancellationToken,Task> _actionToExecute;
        private IErrorHandler _responseErrorHandler = ErrorHandler.NotHandled;
        private IErrorHandler _executionErrorHandler = ErrorHandler.NotHandled;

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
        internal void OnResponseError(IErrorHandler errorHandler)
        {
            _responseErrorHandler = errorHandler;
        }
        internal void OnExecutionError(IErrorHandler errorHandler)
        {
            _executionErrorHandler = errorHandler;
        }
        private async Task ExecuteTasks(HostedWorkflows hostedWorkflows, CancellationToken cancellationToken)
        {
            var workflowType = _decisionTask.WorkflowType;
            var workflow = hostedWorkflows.FindBy(workflowType.Name, workflowType.Version);
            var historyEvents = new WorkflowHistoryEvents(_decisionTask.Events, _decisionTask.StartedEventId, _decisionTask.PreviousStartedEventId);
            using (var execution = workflow.NewExecutionFor(historyEvents))
            {
                var decisions = Perform(execution);
                await SendDecisionsAsync(decisions, cancellationToken);
                RaisePostExecutionEvents(decisions, workflow);
            }
        }

        private WorkflowDecision [] Perform(WorkflowEventsExecution execution)
        {
            var error = new Error();
            int retryAttempts = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    return execution.Execute().ToArray();
                }
                catch (Exception exception)
                {
                    var errorAction = _executionErrorHandler.OnError(error.Set(exception, retryAttempts));
                    if (errorAction.IsRethrow)
                        throw;
                    if (errorAction.IsRetry)
                        retry = true;
                }
                retryAttempts++;
            } while (retry);
            return new WorkflowDecision[0];
        }

        private async Task SendDecisionsAsync(IEnumerable<WorkflowDecision> decisions, CancellationToken cancellationToken)
        {
            var error = new Error();
            int retryAttempts = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    await SendDecisionsToAmazonSwfAsync(decisions, cancellationToken);
                }
                catch (Exception exception)
                {
                    var errorAction = _responseErrorHandler.OnError(error.Set(exception, retryAttempts));
                    if (errorAction.IsRethrow)
                        throw;
                    if (errorAction.IsRetry)
                        retry = true;
                }
                retryAttempts++;
            } while (retry);
        }

        private async Task SendDecisionsToAmazonSwfAsync(IEnumerable<WorkflowDecision> decisions, CancellationToken cancellationToken)
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