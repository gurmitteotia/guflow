using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow
{
    public class Domain
    {
        private readonly string _name;
        private readonly IAmazonSimpleWorkflow _amazonSimpleWorkflow;

        public Domain(string name, IAmazonSimpleWorkflow amazonSimpleWorkflow)
        {
            Ensure.NotNullAndEmpty(name, () => new ArgumentException(Resources.Domain_name_required, "name"));
            Ensure.NotNull(amazonSimpleWorkflow,()=> new ArgumentException(Resources.Amazon_client_required, "amazonSimpleWorkflow"));

            _name = name;
            _amazonSimpleWorkflow = amazonSimpleWorkflow;
        }

        public async Task RegisterWorkflowAsync<TWorkflow>() where TWorkflow : Workflow
        {
            await RegisterWorkflowAsync(typeof (TWorkflow));
        }

        public async Task RegisterWorkflowAsync(Type workflowType)
        {
            await RegisterWorkflowAsync(WorkflowDescriptionAttribute.FindOn(workflowType));
        }

        public async Task RegisterWorkflowAsync(WorkflowDescriptionAttribute workflowDescription)
        {
            Ensure.NotNull(workflowDescription, ()=>new ArgumentException(Resources.Workflow_description_required, "workflowDescription"));

            var registeredWorkflowInfos = await ListWorkflowFromAmazonBy(workflowDescription.Name, _name);

            var workflowToRegister = registeredWorkflowInfos.FirstOrDefault(w => w.WorkflowType.Version.Equals(workflowDescription.Version));

            if (workflowToRegister != null && workflowToRegister.Status == RegistrationStatus.DEPRECATED)
                throw new WorkflowDeprecatedException(string.Format(Resources.Workflow_deprecated, workflowDescription.Name, workflowDescription.Version));

            if (workflowToRegister == null)
                await RegisterWorkflow(workflowDescription);
        }

        public async Task RegisterAsync(uint retentionPeriodInDays, string description = null)
        {
            var domainInfo = await ListDomainFromAmazonBy(_name);
            if(domainInfo != null && domainInfo.Status == RegistrationStatus.DEPRECATED)
                throw new DomainDeprecatedException(string.Format(Resources.Domain_deprecated,_name));
            if (domainInfo == null)
                await RegisterDomainAsync(retentionPeriodInDays, description);
        }

        internal async Task<DecisionTask> PollForDecisionTaskAsync(TaskQueue taskQueue)
        {
            return await taskQueue.ReadStrategy(this, taskQueue, null);
        }

        internal async Task<DecisionTask> PollForDecisionTaskAsync(TaskQueue taskQueue, string nextPageToken)
        {
            var request = taskQueue.CreateRequest(_name, nextPageToken);
            var response = await _amazonSimpleWorkflow.PollForDecisionTaskAsync(request);
            return response.DecisionTask;
        }
        private async Task RegisterDomainAsync(uint retentionPeriodInDays, string description)
        {
            var request = new RegisterDomainRequest()
            {
                Name = _name,
                Description = description,
                WorkflowExecutionRetentionPeriodInDays = retentionPeriodInDays.ToString()
            };
            await _amazonSimpleWorkflow.RegisterDomainAsync(request);
        }

        private async Task<DomainInfo> ListDomainFromAmazonBy(string domainName)
        {
            var request = new ListDomainsRequest() {MaximumPageSize = 1000};
            var response = await _amazonSimpleWorkflow.ListDomainsAsync(request);
            return response.DomainInfos.Infos.FirstOrDefault(d => d.Name.Equals(domainName));
        }

        private async Task RegisterWorkflow(WorkflowDescriptionAttribute workflowDescription)
        {
            var registerRequest = new RegisterWorkflowTypeRequest()
            {
                Name = workflowDescription.Name,
                Version = workflowDescription.Version,
                Description = workflowDescription.Description,
                Domain = _name,
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

        private static TaskList TaskList(string taskListName)
        {
            TaskList taskList = null;
            if (!string.IsNullOrEmpty(taskListName))
                taskList = new TaskList() {Name = taskListName};
            return taskList;
        }

    }
}