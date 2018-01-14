using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow.Decider
{
    /// <summary>
    /// Host the execution of workflows.
    /// </summary>
    public sealed class WorkflowHost : IDisposable, IHost
    {
        private readonly Domain _domain;
        private readonly Workflows _hostedWorkflows;
        private ErrorHandler _responseErrorHandler = ErrorHandler.NotHandled;
        private ErrorHandler _genericErrorHandler = ErrorHandler.Continue;
        private ErrorHandler _pollingErrorHandler = ErrorHandler.NotHandled;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed = false;
        private readonly ILog _log = Log.GetLogger<WorkflowHost>();
        private readonly ManualResetEventSlim _stoppedEvent = new ManualResetEventSlim(false);
        /// <summary>
        /// Create a host for given workflows.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="workflows"></param>
        public WorkflowHost(Domain domain, IEnumerable<Workflow> workflows)
        {
            Ensure.NotNull(domain, "domain");
            Ensure.NotNull(workflows, "workflows");
            PollingIdentity = Environment.MachineName;
            workflows = workflows.Where(w => w != null).ToArray();
            if (!workflows.Any())
                throw new ArgumentException(Resources.No_workflow_to_host, nameof(workflows));
            Status = HostStatus.Initialized;
            _domain = domain;
            _hostedWorkflows = new Workflows(workflows);
            OnPollingError(e=>ErrorAction.Unhandled);
            OnResponseError(e=>ErrorAction.Unhandled);
        }

        internal Workflow FindBy(string name, string version)
        {
            return _hostedWorkflows.FindBy(name, version);
        }
        /// <summary>
        /// Status of workflow host.
        /// </summary>
        public HostStatus Status { get; private set; }
        /// <summary>
        /// Fired when host goes in to faulted state because of unhandled exception.
        /// </summary>
        public event EventHandler<HostFaultEventArgs> OnFault;

        /// <summary>
        /// Start execution of hosted workflow on default task list. It will throw the exception when more than one workflows are hosted.
        /// </summary>
        public void StartExecution()
        {
            var defaultTaskList = DetectTaskList();
            StartExecution(new TaskList(defaultTaskList));
        }
        /// <summary>
        /// Start execution of hosted workflows on given TaskList.
        /// </summary>
        /// <param name="taskList"></param>
        public void StartExecution(TaskList taskList)
        {
            if (_disposed)
                throw new ObjectDisposedException(Resources.Workflow_execution_already_stopped);

            Ensure.NotNull(taskList, "taskList");
            var domain = _domain.OnPollingError(_pollingErrorHandler);
            ExecuteHostedWorkfowsAsync(taskList, domain);
        }
        /// <summary>
        /// Stop the execution of hosted workflows.
        /// </summary>
        public void StopExecution()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cancellationTokenSource.Cancel();
                _stoppedEvent.Wait(TimeSpan.FromSeconds(5));
            }
        }

        public void Dispose()
        {
            StopExecution();
        }
        /// <summary>
        /// Register the error handler for polling error.
        /// </summary>
        /// <param name="handleError"></param>
        public void OnPollingError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _pollingErrorHandler = ErrorHandler.Default(handleError).WithFallback(_genericErrorHandler);
        }
        /// <summary>
        /// Register the error handler for response errror.
        /// </summary>
        /// <param name="handleError"></param>
        public void OnResponseError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _responseErrorHandler = ErrorHandler.Default(handleError).WithFallback(_genericErrorHandler);
        }

        /// <summary>
        /// Generic error handler. If exeception remains unhandled at this stage then host goes into faulted state.
        /// </summary>
        /// <param name="handleError"></param>
        public void OnError(HandleError handleError)
        {
            Ensure.NotNull(handleError, "handleError");
            _genericErrorHandler = ErrorHandler.Default(handleError);
            _responseErrorHandler = _responseErrorHandler.WithFallback(_genericErrorHandler);
            _pollingErrorHandler = _pollingErrorHandler.WithFallback(_genericErrorHandler);
        }

        public string PollingIdentity { get; set; }

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
        private async void ExecuteHostedWorkfowsAsync(TaskList taskList, Domain domain)
        {
            Status = HostStatus.Executing;
            var pollingIdentity = PollingIdentity;
            try
            {
                while (!_disposed)
                {
                    var workflowTask = await PollForTaskAsync(taskList, domain, pollingIdentity);
                    await workflowTask.ExecuteForAsync(this, _cancellationTokenSource.Token);
                }
                Status = HostStatus.Stopped;
            }
            catch (OperationCanceledException)
            {
                _log.Info("Host is shutting down");
                Status = HostStatus.Stopped;
            }
            catch (Exception exception)
            {
                Status = HostStatus.Faulted;
                _log.Fatal("Hosted workflows is faulted.", exception);
                OnFault?.Invoke(this, new HostFaultEventArgs(exception));
            }
            finally
            {
                _stoppedEvent.Set();
            }
        }
        private async Task<WorkflowTask> PollForTaskAsync(TaskList taskList, Domain domain, string pollingIdentity)
        {
            _log.Debug($"Polling for decision task on queue {taskList} and domain {domain}");
            var workflowTask = await taskList.PollForWorkflowTaskAsync(domain, pollingIdentity, _cancellationTokenSource.Token);
            workflowTask.OnExecutionError(_genericErrorHandler);
            return workflowTask;
        }

        private string DetectTaskList()
        {
            var taskLists = _hostedWorkflows.Descriptions.Select(d => d.DefaultTaskListName).ToArray();
            var defaultTaskList = taskLists.FirstOrDefault(f => !string.IsNullOrEmpty(f));
            if (string.IsNullOrEmpty(defaultTaskList))
                throw new InvalidOperationException(Resources.Can_not_determine_the_task_list_to_poll_for_activity_task);

            if (taskLists.Any(f => f != defaultTaskList))
                throw new InvalidOperationException(Resources.Can_not_determine_the_task_list_to_poll_for_activity_task);

            return defaultTaskList;
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
            public IEnumerable<WorkflowDescriptionAttribute> Descriptions
                                                => _workflows.Values.Select(s => WorkflowDescriptionAttribute.FindOn(s.GetType()));
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