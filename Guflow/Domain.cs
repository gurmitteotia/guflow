using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow
{
    public sealed class Domain
    {
        private readonly string _name;
        private readonly IAmazonSimpleWorkflow _simpleWorkflowClient;

        public Domain(string name, IAmazonSimpleWorkflow simpleWorkflowClient)
        {
            Ensure.NotNullAndEmpty(name, () => new ArgumentException(Resources.Domain_name_required, "name"));
            Ensure.NotNull(simpleWorkflowClient, "simpleWorkflowClient");

            _name = name;
            _simpleWorkflowClient = simpleWorkflowClient;
        }

        internal IAmazonSimpleWorkflow Client
        {
            get { return _simpleWorkflowClient; }
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
            Ensure.NotNull(workflowDescription, "workflowDescription");
            var registeredWorkflowInfos = await ListWorkflowFromAmazonBy(workflowDescription.Name, _name);

            var workflowToRegister = registeredWorkflowInfos.FirstOrDefault(w => w.WorkflowType.Version.Equals(workflowDescription.Version));

            if (workflowToRegister != null && workflowToRegister.Status == RegistrationStatus.DEPRECATED)
                throw new WorkflowDeprecatedException(string.Format(Resources.Workflow_deprecated, workflowDescription.Name, workflowDescription.Version));

            if (workflowToRegister == null)
                await _simpleWorkflowClient.RegisterWorkflowTypeAsync(workflowDescription.RegisterRequest(_name));
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
            return await taskQueue.ReadStrategy(this, taskQueue);
        }

        public async Task<DecisionTask> PollForDecisionTaskAsync(TaskQueue taskQueue, string nextPageToken)
        {
            int retryAttempts = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    var request = taskQueue.CreateRequest(_name, nextPageToken);
                    var response = await _simpleWorkflowClient.PollForDecisionTaskAsync(request);
                    return response.DecisionTask;
                }
                catch (Exception exception)
                {
                    var errorAction = taskQueue.HandleError(exception, retryAttempts);
                    if (errorAction.IsRethrow)
                        throw;
                    if (errorAction.IsRetry)
                        retry = true;
                }
                retryAttempts++;
            } while (retry);
            return new DecisionTask();
        }
        
        public HostedWorkflows Host(IEnumerable<Workflow> workflows)
        {
            return new HostedWorkflows(this,workflows);
        }

        public async Task CancelWorkflowAsync(CancelWorkflowRequest cancelRequest)
        {
            Ensure.NotNull(cancelRequest, "cancelRequest");
            await _simpleWorkflowClient.RequestCancelWorkflowExecutionAsync(cancelRequest.SwfFormat(_name));
        }

        public async Task StartWorkflowAsync(StartWorkflowRequest startRequest)
        {
            Ensure.NotNull(startRequest, "startRequest");
            await _simpleWorkflowClient.StartWorkflowExecutionAsync(startRequest.SwfFormat(_name));
        }
        public async Task SignalWorkflowAsync(SignalWorkflowRequest signalRequest)
        {
            Ensure.NotNull(signalRequest, "signalRequest");

            await _simpleWorkflowClient.SignalWorkflowExecutionAsync(signalRequest.SwfFormat(_name));

        }
        private async Task RegisterDomainAsync(uint retentionPeriodInDays, string description)
        {
            var request = new RegisterDomainRequest()
            {
                Name = _name,
                Description = description,
                WorkflowExecutionRetentionPeriodInDays = retentionPeriodInDays.ToString()
            };
            await _simpleWorkflowClient.RegisterDomainAsync(request);
        }

        private async Task<DomainInfo> ListDomainFromAmazonBy(string domainName)
        {
            var request = new ListDomainsRequest() {MaximumPageSize = 1000};
            var response = await _simpleWorkflowClient.ListDomainsAsync(request);
            return response.DomainInfos.Infos.FirstOrDefault(d => d.Name.Equals(domainName));
        }

        private async Task<IEnumerable<WorkflowTypeInfo>> ListWorkflowFromAmazonBy(string workflowName, string domainName)
        {
            var listRequest = new ListWorkflowTypesRequest();
            listRequest.Name = workflowName;
            listRequest.Domain = domainName;
            listRequest.MaximumPageSize = 1000;
            var response = await _simpleWorkflowClient.ListWorkflowTypesAsync(listRequest);
            return response.WorkflowTypeInfos.TypeInfos;
        }
    }
}