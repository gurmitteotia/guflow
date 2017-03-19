using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow.Decider
{
    public sealed class HostedWorkflows : IDisposable
    {
        private readonly Domain _domain;
        private readonly Dictionary<string, Workflow> _hostedWorkflows = new Dictionary<string, Workflow>();
        private volatile bool _cancelled = false;
        private ErrorHandler _responseErrorHandler = ErrorHandler.NotHandled;
        private ErrorHandler _genericErrorHandler = ErrorHandler.NotHandled;
        private ErrorHandler _pollingErrorHandler = ErrorHandler.NotHandled;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed = false;
        public HostedWorkflows(Domain domain, IEnumerable<Workflow> workflows)
        {
            Ensure.NotNull(domain, "domain");
            Ensure.NotNull(workflows, "workflows");

            workflows = workflows.Where(w => w != null).ToArray();
            if(!workflows.Any())
                throw new ArgumentException(Resources.No_workflow_to_host, "workflows");

            _domain = domain;
            
            PopulateHostedWorkflows(workflows);
        }

        internal Workflow FindBy(string name, string version)
        {
            Workflow hostedWorkflow;
            var hostedWorkflowKey = name + version;
            if(!_hostedWorkflows.TryGetValue(hostedWorkflowKey, out hostedWorkflow))
                throw new WorkflowNotHostedException(string.Format(Resources.Workflow_not_hosted,name,version));
            return hostedWorkflow;
        }

        private void PopulateHostedWorkflows(IEnumerable<Workflow> workflows)
        {
            foreach (var workflow in workflows)
            {
                var workflowDescription = WorkflowDescriptionAttribute.FindOn(workflow.GetType());
                var hostedWorkflowKey = workflowDescription.Name + workflowDescription.Version;
                if(_hostedWorkflows.ContainsKey(hostedWorkflowKey))
                    throw new WorkflowAlreadyHostedException(string.Format(Resources.Workflow_already_hosted, workflowDescription.Name,workflowDescription.Version));
                _hostedWorkflows.Add(hostedWorkflowKey,workflow);
            }
        }

        public void StartExecution()
        {
            if (_hostedWorkflows.Count() != 1)
            {
                throw new InvalidOperationException(Resources.Can_not_determine_the_task_list_to_poll_on);
            }
            var singleHostedWorkflow = _hostedWorkflows.Values.First();
            var defaultTaskListName = WorkflowDescriptionAttribute.FindOn(singleHostedWorkflow.GetType()).DefaultTaskListName;
            if(string.IsNullOrEmpty(defaultTaskListName))
                throw new InvalidOperationException(Resources.Default_task_list_is_missing);
            
            StartExecution(new TaskQueue(defaultTaskListName));
        }
        public void StartExecution(TaskQueue taskQueue)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if(_cancelled)
                throw new InvalidOperationException(Resources.Workflow_execution_already_stopped);

            Ensure.NotNull(taskQueue, "taskQueue");
            var domain = _domain.OnPollingError(_pollingErrorHandler);
            ExecuteHostedWorkfowsAsync(taskQueue, domain);
        }

        public void StopExecution()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            _cancelled = true;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopExecution();
            }
        }

        public void OnPollingError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _pollingErrorHandler = ErrorHandler.Default(handleError).WithFallback(_genericErrorHandler);
        }
        public void OnPollingError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            OnPollingError(errorHandler.OnError);
        }

        public void OnResponseError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _responseErrorHandler = ErrorHandler.Default(handleError).WithFallback(_genericErrorHandler);
        }
        public void OnResponseError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            OnResponseError(errorHandler.OnError);
        }
        public void OnError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _genericErrorHandler = ErrorHandler.Default(handleError);
            _responseErrorHandler = _responseErrorHandler.WithFallback(_genericErrorHandler);
            _pollingErrorHandler = _pollingErrorHandler.WithFallback(_genericErrorHandler);
        }
        public void OnError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            OnError(errorHandler.OnError);
        }

        internal async Task SendDecisionsAsync(string taskToken, IEnumerable<WorkflowDecision> decisions)
        {
            var retryAbleFunc = new RetryableFunc(_responseErrorHandler);
            await retryAbleFunc.ExecuteAsync(() => SendDecisionsToAmazonSwfAsync(taskToken, decisions));
        }

        private async Task SendDecisionsToAmazonSwfAsync(string taskToken, IEnumerable<WorkflowDecision> decisions)
        {
            var decisionsResponse = ResponseFrom(taskToken, decisions);
            await _domain.Client.RespondDecisionTaskCompletedAsync(decisionsResponse, _cancellationTokenSource.Token);
        }

        private static RespondDecisionTaskCompletedRequest ResponseFrom(string taskToken, IEnumerable<WorkflowDecision> decisions)
        {
            return new RespondDecisionTaskCompletedRequest
            {
                TaskToken = taskToken,
                Decisions = decisions.Select(s => s.Decision()).ToList()
            };
        }
        private async void ExecuteHostedWorkfowsAsync(TaskQueue taskQueue, Domain domain)
        {
            while (!_cancelled)
            {
                var workflowTask = await PollForTaskAsync(taskQueue, domain);
                await workflowTask.ExecuteForAsync(this, _cancellationTokenSource.Token);
            }
        }
        private async Task<WorkflowTask> PollForTaskAsync(TaskQueue taskQueue, Domain domain)
        {
            var workflowTask = await taskQueue.PollForWorkflowTaskAsync(domain);
            workflowTask.OnExecutionError(_genericErrorHandler);
            return workflowTask;
        }
    }
}