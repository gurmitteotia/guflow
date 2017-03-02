using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Properties;
using Guflow.Worker;

namespace Guflow
{
    public sealed class Domain
    {
        private readonly string _name;
        private readonly IAmazonSimpleWorkflow _simpleWorkflowClient;
        private static readonly DecisionTask DefaultDecisionTask = new DecisionTask();
        private static readonly ActivityTask DefaultActivityTask = new ActivityTask();
        public Domain(string name, IAmazonSimpleWorkflow simpleWorkflowClient)
        {
            Ensure.NotNullAndEmpty(name, () => new ArgumentException(Resources.Domain_name_required, "name"));
            Ensure.NotNull(simpleWorkflowClient, "simpleWorkflowClient");
            _name = name;
            _simpleWorkflowClient = simpleWorkflowClient;
        }
        public Domain(string name, RegionEndpoint regionEndpoint) : this(name, new AmazonSimpleWorkflowClient(regionEndpoint))
        {
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
            Ensure.NotNull(workflowType, "activityType");
            await RegisterWorkflowAsync(WorkflowDescriptionAttribute.FindOn(workflowType));
        }
        public async Task RegisterWorkflowAsync(WorkflowDescriptionAttribute workflowDescription)
        {
            Ensure.NotNull(workflowDescription, "workflowDescription");
            var registeredWorkflowInfos = await ListWorkflowsFromAmazonBy(workflowDescription.Name);

            var workflowToRegister = registeredWorkflowInfos.FirstOrDefault(w => w.WorkflowType.Version.Equals(workflowDescription.Version));

            if (workflowToRegister != null)
                return;

            await _simpleWorkflowClient.RegisterWorkflowTypeAsync(workflowDescription.RegisterRequest(_name));
        }

        public async Task RegisterActivityAsync<TActivity>() where TActivity : Activity
        {
            await RegisterActivityAsync(typeof(TActivity));
        }

        public async Task RegisterActivityAsync(Type activityType)
        {
            Ensure.NotNull(activityType, "activityType");
            await RegisterActivityAsync(ActivityDescriptionAttribute.FindOn(activityType));
        }

        public async Task RegisterActivityAsync(ActivityDescriptionAttribute activityDescription)
        {
            Ensure.NotNull(activityDescription, "activityDescription");
            var registeredActivitiesInfo = await ListActivitiesFromAmazonBy(activityDescription.Name);

            var activityToRegister = registeredActivitiesInfo.FirstOrDefault(w => w.ActivityType.Version.Equals(activityDescription.Version));

            if (activityToRegister != null)
                return;

            await _simpleWorkflowClient.RegisterActivityTypeAsync(activityDescription.RegisterRequest(_name));
        }

        public async Task RegisterAsync(uint retentionPeriodInDays, string description = null)
        {
            var domainInfo = await ListDomainFromAmazonBy(_name, RegistrationStatus.REGISTERED);
            if (domainInfo != null)
                return;
            await RegisterDomainAsync(retentionPeriodInDays, description);
        }

        internal async Task<ActivityTask> PollForActivityTaskAsync(TaskQueue taskQueue)
        {
            Ensure.NotNull(taskQueue, "taskQueue");
            var error = new Error();
            int retryAttempts = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    return await PollAmazonSwfForActivityTaskAsync(taskQueue);
                }
                catch (Exception exception)
                {
                    var errorAction = taskQueue.HandlePollingError(error.Set(exception, retryAttempts));
                    if (errorAction.IsRethrow)
                        throw;
                    if (errorAction.IsRetry)
                        retry = true;
                }
                retryAttempts++;
            } while (retry);
            return DefaultActivityTask;
        }

        private async Task<ActivityTask> PollAmazonSwfForActivityTaskAsync(TaskQueue taskQueue)
        {
            var activityTaskPollingRequest = taskQueue.CreateActivityTaskPollingRequest(_name);
            var response = await _simpleWorkflowClient.PollForActivityTaskAsync(activityTaskPollingRequest);
            return response.ActivityTask;
        }

        internal async Task<DecisionTask> PollForDecisionTaskAsync(TaskQueue taskQueue)
        {
            return await taskQueue.ReadStrategy(this, taskQueue);
        }

        public async Task<DecisionTask> PollForDecisionTaskAsync(TaskQueue taskQueue, string nextPageToken)
        {
            Ensure.NotNull(taskQueue, "taskQueue");
            var error = new Error();
            int retryAttempts = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    return await PollAmazonSwfForDecisionTaskAsync(taskQueue, nextPageToken);
                }
                catch (Exception exception)
                {
                    var errorAction = taskQueue.HandlePollingError(error.Set(exception, retryAttempts));
                    if (errorAction.IsRethrow)
                        throw;
                    if (errorAction.IsRetry)
                        retry = true;
                }
                retryAttempts++;
            } while (retry);
            return DefaultDecisionTask;
        }

        private async Task<DecisionTask> PollAmazonSwfForDecisionTaskAsync(TaskQueue taskQueue, string nextPageToken)
        {
            var request = taskQueue.CreateDecisionTaskPollingRequest(_name, nextPageToken);
            var response = await _simpleWorkflowClient.PollForDecisionTaskAsync(request);
            return response.DecisionTask;
        } 

        public HostedWorkflows Host(IEnumerable<Workflow> workflows)
        {
            return new HostedWorkflows(this,workflows);
        }
        public HostedActivities Host(IEnumerable<Activity> activities)
        {
            return new HostedActivities(this, activities);
        }

        public HostedActivities Host(IEnumerable<Type> activitiesTypes, Func<Type, Activity> instanceCreator)
        {
            return new HostedActivities(this, activitiesTypes, instanceCreator);
        }
        public HostedActivities Host(IEnumerable<Type> activitiesTypes)
        {
            return new HostedActivities(this, activitiesTypes);
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

        private async Task<DomainInfo> ListDomainFromAmazonBy(string domainName, RegistrationStatus registrationStatus)
        {
            var request = new ListDomainsRequest() {RegistrationStatus = registrationStatus};
            var response = await _simpleWorkflowClient.ListDomainsAsync(request);
            return response.DomainInfos.Infos.FirstOrDefault(d => d.Name.Equals(domainName));
        }

        private async Task<IEnumerable<WorkflowTypeInfo>> ListWorkflowsFromAmazonBy(string workflowName)
        {
            var listRequest = new ListWorkflowTypesRequest();
            listRequest.Name = workflowName;
            listRequest.Domain = _name;
            listRequest.MaximumPageSize = 1000;
            listRequest.RegistrationStatus = RegistrationStatus.REGISTERED;
            var response = await _simpleWorkflowClient.ListWorkflowTypesAsync(listRequest);
            return response.WorkflowTypeInfos.TypeInfos;
        }
        private async Task<IEnumerable<ActivityTypeInfo>> ListActivitiesFromAmazonBy(string activityName)
        {
            var listRequest = new ListActivityTypesRequest();
            listRequest.Name = activityName;
            listRequest.Domain = _name;
            listRequest.MaximumPageSize = 1000;
            listRequest.RegistrationStatus = RegistrationStatus.REGISTERED;
            var response = await _simpleWorkflowClient.ListActivityTypesAsync(listRequest);
            return response.ActivityTypeInfos.TypeInfos;
        }
    }
}