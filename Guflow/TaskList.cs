// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Properties;
using Guflow.Worker;

namespace Guflow
{
    /// <summary>
    /// Represents the task list to poll for new decisions/activity task on Amazon SWF.
    /// </summary>
    public sealed class TaskList
    {
        private readonly string _taskListName;
        private ReadHistoryEvents _readHistoryEvents;
        /// <summary>
        /// Strategy to read all history events of workflow.
        /// </summary>
        public static readonly ReadHistoryEvents ReadAllEvents = (d,q,p,t) => ReadAllEventsAsync(d,q,p,t);
        /// <summary>
        /// Strategy to read only first page of workflow history event. Use it carefully.
        /// </summary>
        public static readonly ReadHistoryEvents ReadFirstPage = (d,q,p,t) => ReadFirstPageAsync(d, q, p,t);
        private readonly ILog _log = Log.GetLogger<TaskList>();

        /// <summary>
        /// Create a new instance of TaskList.
        /// </summary>
        /// <param name="taskListName"></param>
        public TaskList(string taskListName)
        {
            Ensure.NotNullAndEmpty(taskListName,()=>new ArgumentException(Resources.TaskListName_required, nameof(taskListName)));
            _taskListName = taskListName;
            //_pollingIdentity = pollingIdentity??Environment.MachineName;
            ReadStrategy = ReadAllEvents;
        }

        /// <summary>
        /// Gets or sets the strategy to read workflow history events. By default it reads all history events of workflow when responding to new 
        /// decision tasks.
        /// </summary>
        public ReadHistoryEvents ReadStrategy
        {
            get => _readHistoryEvents;
            set
            {
                Ensure.NotNull(value,()=>new ArgumentNullException(Resources.Read_strategy_required, "ReadStrategy"));
                _readHistoryEvents = value;
            }
        }

        internal async Task<WorkflowTask> PollForWorkflowTaskAsync(Domain domain, string pollingIdentity, CancellationToken token)
        {
            _log.Debug($"Polling for new decisions on {this} under {domain}");
            var workflowTask = await ReadStrategy(domain, this, pollingIdentity, token);
            if(workflowTask==WorkflowTask.Empty)
                _log.Debug($"No new decisions are returned on {this} under {domain}");
            workflowTask.Validate();
            return workflowTask;
        }

        internal async Task<WorkerTask> PollForWorkerTaskAsync(Domain domain, string pollingIdentity, CancellationToken cancellationToken)
        {
            var activityTask = await domain.PollForActivityTaskAsync(this, pollingIdentity ,cancellationToken);
            if (IsTaskReturned(activityTask))
                return WorkerTask.CreateFor(activityTask, new HeartbeatSwfApi(domain.Client));

            return WorkerTask.Empty;
        }
       
        private static bool IsTaskReturned(ActivityTask activityTask)
        {
            return !string.IsNullOrEmpty(activityTask?.TaskToken);
        }

        private static async Task<WorkflowTask> ReadAllEventsAsync(Domain domain, TaskList taskList, string pollingIdentity, CancellationToken token, string nextPageToken=null)
        {
            var decisionTask = await domain.PollForDecisionTaskAsync(taskList, pollingIdentity , token, nextPageToken);
            var workflowTask = WorkflowTask.Create(decisionTask);
            if (HasMoreEventsToRead(decisionTask))
            {
                var nextDecisionTasks = await ReadAllEventsAsync(domain, taskList, pollingIdentity,token ,decisionTask.NextPageToken);
                workflowTask.Append(nextDecisionTasks);
            }
            return workflowTask;
        }
        private static bool AreDecisionsReturned(DecisionTask decision)
        {
            return !string.IsNullOrEmpty(decision?.TaskToken);
        }

        private static bool HasMoreEventsToRead(DecisionTask decisionTask)
        {
            return !string.IsNullOrEmpty(decisionTask?.NextPageToken);
        }

        private static async Task<WorkflowTask> ReadFirstPageAsync(Domain domain, TaskList taskList, string pollingIdentity, CancellationToken token, string nextPageToken= null)
        {
            var decisionTask = await domain.PollForDecisionTaskAsync(taskList,pollingIdentity ,token,nextPageToken);
            return WorkflowTask.Create(decisionTask);
        }

        internal PollForDecisionTaskRequest DecisionTaskPollingRequest(string domain, string pollingIdentity, string nextPageToken = null)
        {
            return new PollForDecisionTaskRequest
            {
                Identity = pollingIdentity,
                Domain = domain,
                TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = _taskListName },
                MaximumPageSize = 1000,
                ReverseOrder = true,
                NextPageToken = nextPageToken
            };
        }

        internal PollForActivityTaskRequest ActivityTaskPollingRequest(string domain, string pollingIdentity)
        {
            return new PollForActivityTaskRequest()
            {
                Identity = pollingIdentity,
                Domain = domain,
                TaskList = new Amazon.SimpleWorkflow.Model.TaskList() {Name = _taskListName}
            };
        }

        public override string ToString()
        {
            return $"TaskList {_taskListName}";
        }
    }
    public delegate Task<WorkflowTask> ReadHistoryEvents(Domain domain, TaskList taskList, string pollingIdentity, CancellationToken token);
}