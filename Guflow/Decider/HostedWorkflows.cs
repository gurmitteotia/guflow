using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow.Decider
{
    public sealed class HostedWorkflows : IDisposable, IHostedItems
    {
        private readonly Domain _domain;
        private readonly Workflows _hostedWorkflows;
        private volatile bool _cancelled = false;
        private ErrorHandler _responseErrorHandler = ErrorHandler.NotHandled;
        private ErrorHandler _genericErrorHandler = ErrorHandler.NotHandled;
        private ErrorHandler _pollingErrorHandler = ErrorHandler.NotHandled;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed = false;
        private readonly ILog _log = Log.GetLogger<HostedWorkflows>();
        public HostedWorkflows(Domain domain, IEnumerable<Workflow> workflows)
        {
            Ensure.NotNull(domain, "domain");
            Ensure.NotNull(workflows, "workflows");

            workflows = workflows.Where(w => w != null).ToArray();
            if(!workflows.Any())
                throw new ArgumentException(Resources.No_workflow_to_host, nameof(workflows));

            _domain = domain;
            _hostedWorkflows = new Workflows(workflows);
        }

        internal Workflow FindBy(string name, string version)
        {
            return _hostedWorkflows.FindBy(name, version);
        }
        public void StartExecution()
        {
            if (_hostedWorkflows.Count != 1)
            {
                throw new InvalidOperationException(Resources.Can_not_determine_the_task_list_to_poll_for_workflow_decisions);
            }
            var singleHostedWorkflow = _hostedWorkflows.Single();
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
            if (!_cancelled)
            {
                _cancelled = true;
                _cancellationTokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopExecution();
                _cancellationTokenSource.Dispose();
                _disposed = true;
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
            try
            {
                while (!_cancelled)
                {
                    var workflowTask = await PollForTaskAsync(taskQueue, domain);
                    await workflowTask.ExecuteForAsync(this, _cancellationTokenSource.Token);
                }
            }
            catch (Exception exception)
            {
                if (_cancelled && exception is OperationCanceledException)
                {
                    _log.Info("Hosted workflows are shutting down");
                    return;
                }
                _log.Fatal("Hosted workflows is faulted.", exception);
                Environment.FailFast("Hosted workflow is faulted. Bringing down the system.", exception);
            }
        }
        private async Task<WorkflowTask> PollForTaskAsync(TaskQueue taskQueue, Domain domain)
        {
            var workflowTask = await taskQueue.PollForWorkflowTaskAsync(domain);
            workflowTask.OnExecutionError(_genericErrorHandler);
            return workflowTask;
        }

        private class Workflows
        {
            private readonly Dictionary<string, Workflow> _workflows = new Dictionary<string, Workflow>();

            public Workflows(IEnumerable<Workflow> workflows)
            {
                PopulateHostedWorkflows(workflows);
            }

            public Workflow FindBy(string name, string version)
            {
                Workflow hostedWorkflow;
                var hostedWorkflowKey = name + version;
                if (!_workflows.TryGetValue(hostedWorkflowKey, out hostedWorkflow))
                    throw new WorkflowNotHostedException(string.Format(Resources.Workflow_not_hosted, name, version));
                return hostedWorkflow;
            }

            public int Count
            {
                get { return _workflows.Count; }
            }

            public Workflow Single()
            {
                return _workflows.Values.First();
            }
            private void PopulateHostedWorkflows(IEnumerable<Workflow> workflows)
            {
                foreach (var workflow in workflows)
                {
                    var workflowDescription = WorkflowDescriptionAttribute.FindOn(workflow.GetType());
                    var hostedWorkflowKey = workflowDescription.Name + workflowDescription.Version;
                    if (_workflows.ContainsKey(hostedWorkflowKey))
                        throw new WorkflowAlreadyHostedException(string.Format(Resources.Workflow_already_hosted, workflowDescription.Name, workflowDescription.Version));
                    _workflows.Add(hostedWorkflowKey, workflow);
                }
            }
        }
    }
}