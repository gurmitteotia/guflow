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
        private readonly string _pollingIdentity;
        private ReadHistoryEvents _readHistoryEvents;
        /// <summary>
        /// Strategy to read all history events of workflow.
        /// </summary>
        public static readonly ReadHistoryEvents ReadAllEvents = (d,q) => ReadAllEventsAsync(d,q);
        /// <summary>
        /// Strategy to read only first page of workflow history event. Use it carefully.
        /// </summary>
        public static readonly ReadHistoryEvents ReadFirstPage = (d,q) => ReadFirstPageAsync(d, q);
        private readonly ILog _log = Log.GetLogger<TaskList>();
        /// <summary>
        /// Create a new instance of TaskList.
        /// </summary>
        /// <param name="taskListName"></param>
        /// <param name="pollingIdentity"></param>
        public TaskList(string taskListName, string pollingIdentity = null)
        {
            Ensure.NotNullAndEmpty(taskListName,()=>new ArgumentException(Resources.TaskListName_required, nameof(taskListName)));
            _taskListName = taskListName;
            _pollingIdentity = pollingIdentity??Environment.MachineName;
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

        internal async Task<WorkflowTask> PollForWorkflowTaskAsync(Domain domain)
        {
            _log.Debug($"Polling for new decisions on {this} under {domain}");
            var decisionTask = await domain.PollForDecisionTaskAsync(this);
            if (AreNewDecisionsAreReturned(decisionTask))
                return WorkflowTask.CreateFor(decisionTask, domain);

            _log.Debug($"No new decisions are returned on {this} under {domain}");
            return WorkflowTask.Empty;
        }

        internal async Task<WorkerTask> PollForWorkerTaskAsync(Domain domain, CancellationToken cancellationToken)
        {
            var activityTask = await domain.PollForActivityTaskAsync(this, cancellationToken);
            if (NewTasksAreReturned(activityTask))
                return WorkerTask.CreateFor(activityTask);

            return WorkerTask.Empty;
        }
       
        private static bool NewTasksAreReturned(ActivityTask activityTask)
        {
            return !string.IsNullOrEmpty(activityTask?.TaskToken);
        }

        private static async Task<DecisionTask> ReadAllEventsAsync(Domain domain, TaskList taskList, string nextPageToken=null)
        {
            var decisionTask = await domain.PollForDecisionTaskAsync(taskList, nextPageToken);
            if (HasMoreEventsToRead(decisionTask))
            {
                var nextDecisionTasks = await ReadAllEventsAsync(domain, taskList ,decisionTask.NextPageToken);
                decisionTask.Events.AddRange(nextDecisionTasks.Events);
            }
            return decisionTask;
        }
        private static bool AreNewDecisionsAreReturned(DecisionTask decision)
        {
            return !string.IsNullOrEmpty(decision?.TaskToken);
        }

        private static bool HasMoreEventsToRead(DecisionTask decisionTask)
        {
            return !string.IsNullOrEmpty(decisionTask?.NextPageToken);
        }

        private static async Task<DecisionTask> ReadFirstPageAsync(Domain domain, TaskList taskList, string nextPageToken= null)
        {
            return await domain.PollForDecisionTaskAsync(taskList, nextPageToken);
        }

        internal PollForDecisionTaskRequest CreateDecisionTaskPollingRequest(string domain, string nextPageToken = null)
        {
            return new PollForDecisionTaskRequest
            {
                Identity = _pollingIdentity,
                Domain = domain,
                TaskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = _taskListName },
                MaximumPageSize = 1000,
                ReverseOrder = true,
                NextPageToken = nextPageToken
            };
        }

        internal PollForActivityTaskRequest CreateActivityTaskPollingRequest(string forDomain)
        {
            return new PollForActivityTaskRequest()
            {
                Identity = _pollingIdentity,
                Domain = forDomain,
                TaskList = new Amazon.SimpleWorkflow.Model.TaskList() {Name = _taskListName}
            };
        }

        public override string ToString()
        {
            return $"TaskList {_taskListName} Identity {_pollingIdentity}";
        }
    }
    public delegate Task<DecisionTask> ReadHistoryEvents(Domain domain, TaskList taskList);
}