using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Properties;
using Guflow.Worker;

namespace Guflow
{
    public sealed class TaskQueue
    {
        private readonly string _taskListName;
        private readonly string _pollingIdentity;
        private ReadHistoryEvents _readHistoryEvents;
        public static readonly ReadHistoryEvents ReadAllEvents = (d,q) => ReadAllEventsAsync(d,q);
        public static readonly ReadHistoryEvents ReadFirstPage = (d,q) => ReadFirstPageAsync(d, q);
        private ILog _log = Log.GetLogger<TaskQueue>();
        public TaskQueue(string taskListName, string pollingIdentity = null)
        {
            Ensure.NotNullAndEmpty(taskListName,()=>new ArgumentException(Resources.TaskListName_required, nameof(taskListName)));
            _taskListName = taskListName;
            _pollingIdentity = pollingIdentity??Environment.MachineName;
            ReadStrategy = ReadAllEvents;
        }

        public ReadHistoryEvents ReadStrategy
        {
            get { return _readHistoryEvents; }
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
            if (NewTasksAreReturned(decisionTask))
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
       
        private static bool NewTasksAreReturned(DecisionTask decisionTask)
        {
            return !string.IsNullOrEmpty(decisionTask.TaskToken);
        }
        private static bool NewTasksAreReturned(ActivityTask activityTask)
        {
            return !string.IsNullOrEmpty(activityTask?.TaskToken);
        }

        private static async Task<DecisionTask> ReadAllEventsAsync(Domain domain, TaskQueue taskQueue, string nextPageToken=null)
        {
            var decisionTask = await domain.PollForDecisionTaskAsync(taskQueue, nextPageToken);
            if (!string.IsNullOrEmpty(decisionTask.NextPageToken))
            {
                var nextDecisionTasks = await ReadAllEventsAsync(domain, taskQueue ,decisionTask.NextPageToken);
                decisionTask.Events.AddRange(nextDecisionTasks.Events);
            }
            return decisionTask;
        }

        private static async Task<DecisionTask> ReadFirstPageAsync(Domain domain, TaskQueue taskQueue, string nextPageToken= null)
        {
            return await domain.PollForDecisionTaskAsync(taskQueue, nextPageToken);
        }

        internal PollForDecisionTaskRequest CreateDecisionTaskPollingRequest(string domain, string nextPageToken = null)
        {
            return new PollForDecisionTaskRequest
            {
                Identity = _pollingIdentity,
                Domain = domain,
                TaskList = new TaskList() { Name = _taskListName },
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
                TaskList = new TaskList() {Name = _taskListName}
            };
        }

        public override string ToString()
        {
            return $"TaskQueue {_taskListName} Identity {_pollingIdentity}";
        }

        public delegate Task<DecisionTask> ReadHistoryEvents(Domain domain, TaskQueue taskQueue);
    }
}