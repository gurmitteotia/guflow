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

        public async Task Register(Type workflowType)
        {
            var workflowDescription = WorkflowDescriptionAttribute.FindOn(workflowType);

            var registeredWorkflowInfos = await ListWorkflowFromAmazonBy();

            var workflowToRegister = registeredWorkflowInfos.FirstOrDefault(w => w.WorkflowType.Version.Equals(workflowDescription.Version));

            if(workflowToRegister!=null && workflowToRegister.Status==RegistrationStatus.DEPRECATED)
                throw new WorkflowDeprecatedException(string.Format(Resources.Workflow_deprecated,workflowDescription.Name,workflowDescription.Version));

            await RegisterWorkflow(workflowDescription);
        }

        private async Task RegisterWorkflow(WorkflowDescriptionAttribute workflowDescription)
        {
            var registerRequest = new RegisterWorkflowTypeRequest()
            {
                Name = workflowDescription.Name,
                Version = workflowDescription.Version,
                Description = workflowDescription.Description,
                Domain = workflowDescription.Domain,
            };

            await _amazonSimpleWorkflow.RegisterWorkflowTypeAsync(registerRequest);
        }

        private async Task<IEnumerable<WorkflowTypeInfo>> ListWorkflowFromAmazonBy()
        {
            var listRequest = new ListWorkflowTypesRequest();

            var response = await _amazonSimpleWorkflow.ListWorkflowTypesAsync(listRequest);
            return response.WorkflowTypeInfos.TypeInfos;
        } 
    }
}