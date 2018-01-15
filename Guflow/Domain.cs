using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Properties;
using Guflow.Worker;

namespace Guflow
{
    /// <summary>
    /// Represents the Amazon SWF domain.
    /// </summary>
    public sealed class Domain
    {
        private readonly string _name;
        private readonly IAmazonSimpleWorkflow _simpleWorkflowClient;
        private readonly IErrorHandler _errorHandler;
        internal static readonly DecisionTask EmptyDecisionTask = new DecisionTask();
        internal static readonly ActivityTask EmptyActivityTask = new ActivityTask();
        /// <summary>
        /// Create a domain with given name and a client to communicate with Amazon SWF. You have more control on creating/configuring the AmazonSimpleWorkflowClient.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="simpleWorkflowClient"></param>
        public Domain(string name, IAmazonSimpleWorkflow simpleWorkflowClient)
            :this(name, simpleWorkflowClient, ErrorHandler.NotHandled)
        {
             
        }
        /// <summary>
        /// Create a domain with given name and in given region. You need to provide credentials in config file.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="regionEndpoint"></param>
        public Domain(string name, RegionEndpoint regionEndpoint) : this(name, new AmazonSimpleWorkflowClient(regionEndpoint))
        {
        }
        private Domain(string name, IAmazonSimpleWorkflow simpleWorkflowClient, IErrorHandler errorHandler)
        {
            Ensure.NotNullAndEmpty(name, () => new ArgumentException(Resources.Domain_name_required, nameof(name)));
            Ensure.NotNull(simpleWorkflowClient, "simpleWorkflowClient");
            _name = name;
            _simpleWorkflowClient = simpleWorkflowClient;
            _errorHandler = errorHandler;
        }
        internal IAmazonSimpleWorkflow Client => _simpleWorkflowClient;

        /// <summary>
        /// Register the TWorkflow with Amazon SWF if not already registered. It will use the information from WorkflowDescriptionAttribute.
        /// </summary>
        /// <typeparam name="TWorkflow">Workflow type to be registered.</typeparam>
        /// <returns></returns>
        public async Task RegisterWorkflowAsync<TWorkflow>() where TWorkflow : Workflow
        {
            await RegisterWorkflowAsync(typeof (TWorkflow));
        }
        /// <summary>
        /// Register the TWorkflow with Amazon SWF if not already registered. It will use the information from WorkflowDescriptionAttribute.
        /// </summary>
        /// <param name="workflowType">Workflow type to be registerd.</param>
        /// <returns></returns>
        public async Task RegisterWorkflowAsync(Type workflowType)
        {
            Ensure.NotNull(workflowType, "activityType");
            await RegisterWorkflowAsync(WorkflowDescriptionAttribute.FindOn(workflowType));
        }
        private async Task RegisterWorkflowAsync(WorkflowDescriptionAttribute workflowDescription)
        {
            Ensure.NotNull(workflowDescription, "workflowDescription");
            var registeredWorkflowInfos = await ListWorkflowsFromAmazonBy(workflowDescription.Name);

            var workflowToRegister = registeredWorkflowInfos.FirstOrDefault(w => w.WorkflowType.Version.Equals(workflowDescription.Version));

            if (workflowToRegister != null)
                return;

            await _simpleWorkflowClient.RegisterWorkflowTypeAsync(workflowDescription.RegisterRequest(_name));
        }
        /// <summary>
        /// Register the activity with Amazon SWF if not already registered. It will use the information from ActivityDescriptionAttribute.
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <returns></returns>
        public async Task RegisterActivityAsync<TActivity>() where TActivity : Activity
        {
            await RegisterActivityAsync(typeof(TActivity));
        }
        /// <summary>
        /// Register the activity with Amazon SWF if not already registered. It will use the information from ActivityDescriptionAttribute.
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        public async Task RegisterActivityAsync(Type activityType)
        {
            Ensure.NotNull(activityType, "activityType");
            await RegisterActivityAsync(ActivityDescriptionAttribute.FindOn(activityType));
        }

        private async Task RegisterActivityAsync(ActivityDescriptionAttribute activityDescription)
        {
            Ensure.NotNull(activityDescription, "activityDescription");
            var registeredActivitiesInfo = await ListActivitiesFromAmazonBy(activityDescription.Name);

            var activityToRegister = registeredActivitiesInfo.FirstOrDefault(w => w.ActivityType.Version.Equals(activityDescription.Version));

            if (activityToRegister != null)
                return;

            await _simpleWorkflowClient.RegisterActivityTypeAsync(activityDescription.RegisterRequest(_name));
        }
        /// <summary>
        /// Register the domain with Amazon SWF if not already registered.
        /// </summary>
        /// <param name="retentionPeriodInDays"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public async Task RegisterAsync(uint retentionPeriodInDays, string description = null)
        {
            var domainInfo = await ListDomainFromAmazonBy(_name, RegistrationStatus.REGISTERED);
            if (domainInfo != null)
                return;
            await RegisterDomainAsync(retentionPeriodInDays, description);
        }

        internal async Task<ActivityTask> PollForActivityTaskAsync(TaskList taskList, string pollingIdentity, CancellationToken cancellationToken)
        {
            Ensure.NotNull(taskList, "taskList");
            var retryableFunc = new RetryableFunc(_errorHandler);
            return  await retryableFunc.ExecuteAsync(
                    async () => await PollAmazonSwfForActivityTaskAsync(taskList, pollingIdentity ,cancellationToken),
                    EmptyActivityTask);
        }

        internal Domain OnPollingError(IErrorHandler errorHandler)
        {
            return new Domain(_name, _simpleWorkflowClient, errorHandler);
        }
        internal Domain OnPollingError(HandleError handleError)
        {
            return new Domain(_name, _simpleWorkflowClient, ErrorHandler.Default(handleError));
        }

        private async Task<ActivityTask> PollAmazonSwfForActivityTaskAsync(TaskList taskList, string pollingIdentity, CancellationToken cancellationToken)
        {
            var request = taskList.CreateActivityTaskPollingRequest(_name, pollingIdentity);
            var response = await _simpleWorkflowClient.PollForActivityTaskAsync(request, cancellationToken);
            return response?.ActivityTask;
        }

        public async Task<DecisionTask> PollForDecisionTaskAsync(TaskList taskList, string pollingIdentity, CancellationToken token, string nextPageToken = null)
        {
            Ensure.NotNull(taskList, "taskList");
            var retryableFunc = new RetryableFunc(_errorHandler);
            return await retryableFunc.ExecuteAsync(
                    async () => await PollAmazonSwfForDecisionTaskAsync(taskList, pollingIdentity , token,nextPageToken),
                    EmptyDecisionTask);
        }

        private async Task<DecisionTask> PollAmazonSwfForDecisionTaskAsync(TaskList taskList, string pollingIdentity, CancellationToken token, string nextPageToken)
        {
            var request = taskList.CreateDecisionTaskPollingRequest(_name, pollingIdentity, nextPageToken);
            var response = await _simpleWorkflowClient.PollForDecisionTaskAsync(request, token);
            return response?.DecisionTask;
        } 

        /// <summary>
        /// Create a host for workflows.
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public WorkflowHost Host(IEnumerable<Workflow> workflows)
        {
            return new WorkflowHost(this,workflows);
        }
        /// <summary>
        /// Create host for activities.
        /// </summary>
        /// <param name="activitiesTypes">List of activities to host.</param>
        /// <param name="instanceCreator">Factory to create activity instances.</param>
        /// <returns></returns>
        public ActivityHost Host(IEnumerable<Type> activitiesTypes, Func<Type, Activity> instanceCreator)
        {
            return new ActivityHost(this, activitiesTypes, instanceCreator);
        }
        /// <summary>
        /// Create the host for activities. It expect activity to have parameterless constructure.
        /// </summary>
        /// <param name="activitiesTypes">List of activities to host.</param>
        /// <returns></returns>
        public ActivityHost Host(IEnumerable<Type> activitiesTypes)
        {
            return new ActivityHost(this, activitiesTypes);
        }
        /// <summary>
        /// Issue a cancellation request to running workflow.
        /// </summary>
        /// <param name="cancelRequest"></param>
        /// <returns></returns>
        public async Task CancelWorkflowAsync(CancelWorkflowRequest cancelRequest)
        {
            Ensure.NotNull(cancelRequest, "cancelRequest");
            await _simpleWorkflowClient.RequestCancelWorkflowExecutionAsync(cancelRequest.SwfFormat(_name));
        }
        /// <summary>
        /// Start a workflow in Amazon SWF. This workflow must be already registered with Amazon SWF.
        /// </summary>
        /// <param name="startRequest"></param>
        /// <returns></returns>
        public async Task<string> StartWorkflowAsync(StartWorkflowRequest startRequest)
        {
            Ensure.NotNull(startRequest, "startRequest");
            var response = await _simpleWorkflowClient.StartWorkflowExecutionAsync(startRequest.SwfFormat(_name));
            return response.Run.RunId;
        }
        /// <summary>
        /// Send a signal to running workflow.
        /// </summary>
        /// <param name="signalRequest"></param>
        /// <returns></returns>
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
        public override string ToString()
        {
            return $"Domain {_name}";
        }
    }
}