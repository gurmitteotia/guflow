using System;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Properties;

namespace Guflow
{
    public sealed class TaskQueue
    {
        private readonly string _taskListName;
        private readonly string _pollingIdentity;
        private ReadHistoryEvents _readHistoryEvents;
        private IErrorHandler _errorHandler = ErrorHandler.NotHandled;
        public static readonly ReadHistoryEvents ReadAllEvents = (d,q) => ReadAllEventsAsync(d,q);
        public static readonly ReadHistoryEvents ReadFirstPage = (d,q) => ReadFirstPageAsync(d, q); 

        public TaskQueue(string taskListName, string pollingIdentity = null)
        {
            Ensure.NotNullAndEmpty(taskListName,()=>new ArgumentException(Resources.TaskListName_required, "taskListName"));
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

        internal async Task<WorkflowTask> PollForNewTasksAsync(Domain domain)
        {
            var decisionTask = await domain.PollForDecisionTaskAsync(this);
            if (NewTasksAreReturned(decisionTask))
                return WorkflowTask.CreateFor(decisionTask, domain);
            return WorkflowTask.Empty;
        }
        public void OnError(HandleError errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            _errorHandler = ErrorHandler.Default(errorHandler);
        }
        public void OnError(IErrorHandler errorHandler)
        {
            Ensure.NotNull(errorHandler, "errorHandler");
            OnError(errorHandler.OnError);
        }
        internal ErrorAction HandlePollingError(Error error)
        {
            return _errorHandler.OnError(error);
        }
        private static bool NewTasksAreReturned(DecisionTask decisionTask)
        {
            return !string.IsNullOrEmpty(decisionTask.TaskToken);
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

        public delegate Task<DecisionTask> ReadHistoryEvents(Domain domain, TaskQueue taskQueue);
    }
}