using System;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow
{
    public class TaskQueue
    {
        private readonly string _taskListName;
        private readonly string _pollingIdentity;
        private ReadHistoryEvents _readHistoryEvents;
        private OnError _onPollingError = (e, c) => ErrorAction.Unhandled;
        public static readonly ReadHistoryEvents ReadAllEvents = ReadAllEventsAsync;
        public static readonly ReadHistoryEvents ReadFirstPage = ReadFirstPageAsync; 

        public TaskQueue(string taskListName, string pollingIdentity = null)
        {
            Ensure.NotNullAndEmpty(taskListName,()=>new ArgumentException(Resources.TaskListName_required, "taskListName"));
            _taskListName = taskListName;
            _pollingIdentity = pollingIdentity??Environment.MachineName;
            _readHistoryEvents = ReadAllEvents;
        }

        public ReadHistoryEvents ReadStrategy
        {
            get { return _readHistoryEvents; }
            set
            {
                Ensure.NotNull(value,()=>new ArgumentException(Resources.Read_strategy_required, "ReadStrategy"));
                _readHistoryEvents = value;
            }
        }

        internal async Task<WorkflowTask> PollForNewTasksAsync(Domain domain)
        {
            var decisionTask = await domain.PollForDecisionTaskAsync(this);
            if (NewTasksAreReturned(decisionTask))
                return WorkflowTask.CreateFor(decisionTask);
            return WorkflowTask.Empty;
        }
        public void OnPollingError(OnError onError)
        {
            Ensure.NotNull(onError,()=>new ArgumentException("onError", "onError"));
            _onPollingError = onError;
        }

        internal ErrorAction HandleError(Exception exception, int count)
        {
            return _onPollingError(exception, count);
        }
        private static bool NewTasksAreReturned(DecisionTask decisionTask)
        {
            return !string.IsNullOrEmpty(decisionTask.TaskToken);
        }

        private static async Task<DecisionTask> ReadAllEventsAsync(Domain domain, TaskQueue taskQueue, string nextPageToken)
        {
            var decisionTask = await domain.PollForDecisionTaskAsync(taskQueue, nextPageToken);
            if (!string.IsNullOrEmpty(decisionTask.NextPageToken))
            {
                var nextDecisionTasks = await ReadAllEventsAsync(domain, taskQueue ,decisionTask.NextPageToken);
                decisionTask.Events.AddRange(nextDecisionTasks.Events);
            }
            return decisionTask;
        }

        private static async Task<DecisionTask> ReadFirstPageAsync(Domain domain, TaskQueue taskQueue, string nextPageToken)
        {
            return await domain.PollForDecisionTaskAsync(taskQueue, nextPageToken);
        }

        internal PollForDecisionTaskRequest CreateRequest(string domain, string nextPageToken = null)
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

        public delegate Task<DecisionTask> ReadHistoryEvents(Domain domain, TaskQueue taskQueue, string nextPageToken);

        public delegate ErrorAction OnError(Exception exception, int count);

    }
}