using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow
{
    public class WorkflowClient : IWorkflowClient
    {
        private readonly IAmazonSimpleWorkflow _amazonSimpleWorkflow;
        private string _identity;
        public WorkflowClient():this(new AmazonSimpleWorkflowClient())
        {
        }
        public WorkflowClient(IAmazonSimpleWorkflow amazonSimpleWorkflow)
        {
            _amazonSimpleWorkflow = amazonSimpleWorkflow;
        }

        public string Domain { get; set; }
        public string TaskListName { get; set; }

        public string Identity
        {
            get { return _identity ?? Environment.MachineName; }
            set { _identity = value; }
        }

        public async Task Register<T>() where T: Workflow
        {
            await Register(typeof(T));
        }

        public async Task Register(WorkflowDescription workflowDescription)
        {
            Validate(workflowDescription);
            var registeredWorkflowInfos = await ListWorkflowFromAmazonBy(workflowDescription.Name, DomainName(workflowDescription.Domain));

            var workflowToRegister = registeredWorkflowInfos.FirstOrDefault(w => w.WorkflowType.Version.Equals(workflowDescription.Version));

            if (workflowToRegister != null && workflowToRegister.Status == RegistrationStatus.DEPRECATED)
                throw new WorkflowDeprecatedException(string.Format(Resources.Workflow_deprecated, workflowDescription.Name, workflowDescription.Version));

            if (workflowToRegister == null)
                await RegisterWorkflow(workflowDescription);
        }

        public async Task Register(Type workflowType)
        {
            var workflowDescription = WorkflowDescriptionAttribute.FindOn(workflowType);
            await Register(workflowDescription);
        }

        public Task RespondWithDecisions(string taskToken, IEnumerable<Decision> decisions)
        {
            throw new NotImplementedException();
        }
        public WorkflowHost CreateHostFor(Workflow workflow)
        {
            throw new NotImplementedException();
        }

        public async Task<WorkflowTasks> PollForNewTasks()
        {
            var response = await PollForTasks();
            
            if(NewTasksAreReturned(response.DecisionTask))
                return WorkflowTasks.CreateFor(response.DecisionTask,this);

            return WorkflowTasks.Empty;
        }

        public async Task<DecisionTask> RetreiveRemainingEvents(string nextPageToken)
        {
            var response = await PollForTasks(nextPageToken);
            return response.DecisionTask;
        }

        private async Task<PollForDecisionTaskResponse> PollForTasks(string nextPageToken = null)
        {
            var request = new PollForDecisionTaskRequest()
            {
                Identity = Identity,
                Domain = Domain,
                TaskList = new TaskList() { Name = TaskListName },
                MaximumPageSize = 1000,
                ReverseOrder = true,
                NextPageToken = nextPageToken
            };
            return await _amazonSimpleWorkflow.PollForDecisionTaskAsync(request);
        }

        private static bool NewTasksAreReturned(DecisionTask decisionTask)
        {
            return !string.IsNullOrEmpty(decisionTask.TaskToken);
        }

        private async Task RegisterWorkflow(WorkflowDescription workflowDescription)
        {
            var registerRequest = new RegisterWorkflowTypeRequest()
            {
                Name = workflowDescription.Name,
                Version = workflowDescription.Version,
                Description = workflowDescription.Description,
                Domain = DomainName(workflowDescription.Domain),
                DefaultExecutionStartToCloseTimeout = workflowDescription.DefaultExecutionStartToCloseTimeout,
                DefaultTaskList = TaskList(workflowDescription.DefaultTaskListName),
                DefaultTaskStartToCloseTimeout = workflowDescription.DefaultTaskStartToCloseTimeout,
                DefaultChildPolicy = workflowDescription.DefaultChildPolicy,
                DefaultLambdaRole = workflowDescription.DefaultLambdaRole,
                DefaultTaskPriority = workflowDescription.DefaultTaskPriority.ToString()
            };

            await _amazonSimpleWorkflow.RegisterWorkflowTypeAsync(registerRequest);
        }

        private async Task<IEnumerable<WorkflowTypeInfo>> ListWorkflowFromAmazonBy(string workflowName, string domainName)
        {
            var listRequest = new ListWorkflowTypesRequest();
            listRequest.Name = workflowName;
            listRequest.Domain = domainName;
            listRequest.MaximumPageSize = 1000;
            var response = await _amazonSimpleWorkflow.ListWorkflowTypesAsync(listRequest);
            return response.WorkflowTypeInfos.TypeInfos;
        }

        private void Validate(WorkflowDescription workflowDescription)
        {
            if(string.IsNullOrEmpty(workflowDescription.Domain)&& string.IsNullOrEmpty(Domain))
                throw new ConfigurationErrorsException(Resources.Domain_name_missing);
        }

        private string DomainName(string defaultDomainName)
        {
            return string.IsNullOrEmpty(Domain) ? defaultDomainName : Domain;
        }
        private TaskList TaskList(string defaultTaskList)
        {
            if (!string.IsNullOrEmpty(TaskListName))
                return new TaskList() {Name = TaskListName};
            if (!string.IsNullOrEmpty(defaultTaskList))
                return new TaskList() {Name = defaultTaskList};
            return null;
        }
    }
}