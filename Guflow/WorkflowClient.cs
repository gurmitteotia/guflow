using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow
{
    public class WorkflowClient
    {
        private readonly IAmazonSimpleWorkflow _amazonSimpleWorkflow;
        public WorkflowClient(IAmazonSimpleWorkflow amazonSimpleWorkflow)
        {
            _amazonSimpleWorkflow = amazonSimpleWorkflow;
        }

        public async Task Register<T>() where T: Workflow
        {
            await Register(typeof(T));
        }

        public async Task Register(WorkflowDescription workflowDescription)
        {
            var registeredWorkflowInfos = await ListWorkflowFromAmazonBy(workflowDescription.Name, workflowDescription.Domain);

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

        private async Task RegisterWorkflow(WorkflowDescription workflowDescription)
        {
            var registerRequest = new RegisterWorkflowTypeRequest()
            {
                Name = workflowDescription.Name,
                Version = workflowDescription.Version,
                Description = workflowDescription.Description,
                Domain = workflowDescription.Domain,
                DefaultExecutionStartToCloseTimeout = workflowDescription.DefaultExecutionStartToCloseTimeout,
                DefaultTaskList = new TaskList() { Name = workflowDescription.DefaultTaskListName},
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
    }
}