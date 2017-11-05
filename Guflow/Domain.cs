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
       
        public Domain(string name, IAmazonSimpleWorkflow simpleWorkflowClient)
            :this(name, simpleWorkflowClient, ErrorHandler.NotHandled)
        {
        }
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
        /// Register the TWorkflow with Amazon SWF. It will use the information from WorkflowDescriptionAttribute.
        /// Registers the workflow if it is not already registered with Amazon SWF.
        /// </summary>
        /// <typeparam name="TWorkflow">Workflow type to be registered.</typeparam>
        /// <returns></returns>
        public async Task RegisterWorkflowAsync<TWorkflow>() where TWorkflow : Workflow
        {
            await RegisterWorkflowAsync(typeof (TWorkflow));
        }
        /// <summary>
        /// Register the TWorkflow with Amazon SWF. It will use the information from WorkflowDescriptionAttribute.
        /// Registers the workflow if it is not already registered with Amazon SWF.
        /// </summary>
        /// <param name="workflowType">Workflow type to be registerd.</param>
        /// <returns></returns>
        public async Task RegisterWorkflowAsync(Type workflowType)
        {
            Ensure.NotNull(workflowType, "activityType");
            await RegisterWorkflowAsync(WorkflowDescriptionAttribute.FindOn(workflowType));
        }
        /// <summary>
        /// Register the workflow with Amazon SWF. This overloaded method is useful when you want to dynamically provide the workflow information.
        /// Registers the workflow if it is not already registered with Amazon SWF.
        /// </summary>
        /// <param name="workflowDescription"></param>
        /// <returns></returns>
        public async Task RegisterWorkflowAsync(WorkflowDescriptionAttribute workflowDescription)
        {
            Ensure.NotNull(workflowDescription, "workflowDescription");
            var registeredWorkflowInfos = await ListWorkflowsFromAmazonBy(workflowDescription.Name);

            var workflowToRegister = registeredWorkflowInfos.FirstOrDefault(w => w.WorkflowType.Version.Equals(workflowDescription.Version));

            if (workflowToRegister != null)
                return;

            await _simpleWorkflowClient.RegisterWorkflowTypeAsync(workflowDescription.RegisterRequest(_name));
        }
        /// <summary>
        /// Register the activity with Amazon SWF. It will use the information from ActivityDescriptionAttribute.
        /// Registers the activity only if it is not already registered with Amazon SWF.
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <returns></returns>
        public async Task RegisterActivityAsync<TActivity>() where TActivity : Activity
        {
            await RegisterActivityAsync(typeof(TActivity));
        }
        /// <summary>
        /// Register the activity with Amazon SWF. It will use the information from ActivityDescriptionAttribute.
        /// Registers the activity only if it is not already registered with Amazon SWF.
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        public async Task RegisterActivityAsync(Type activityType)
        {
            Ensure.NotNull(activityType, "activityType");
            await RegisterActivityAsync(ActivityDescriptionAttribute.FindOn(activityType));
        }

        /// <summary>
        /// Register the activity with Amazon SWF. This overloaded method is useful when you want to dynamically provide the activity information.
        /// Registers the activity if it is not already registered with Amazon SWF.
        /// </summary>
        /// <param name="activityDescription"></param>
        /// <returns></returns>
        public async Task RegisterActivityAsync(ActivityDescriptionAttribute activityDescription)
        {
            Ensure.NotNull(activityDescription, "activityDescription");
            var registeredActivitiesInfo = await ListActivitiesFromAmazonBy(activityDescription.Name);

            var activityToRegister = registeredActivitiesInfo.FirstOrDefault(w => w.ActivityType.Version.Equals(activityDescription.Version));

            if (activityToRegister != null)
                return;

            await _simpleWorkflowClient.RegisterActivityTypeAsync(activityDescription.RegisterRequest(_name));
        }
        /// <summary>
        /// Register the domain with Amazon SWF. Register only when it is not already registered.
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

        internal async Task<ActivityTask> PollForActivityTaskAsync(TaskQueue taskQueue, CancellationToken cancellationToken)
        {
            Ensure.NotNull(taskQueue, "taskQueue");
            var retryableFunc = new RetryableFunc(_errorHandler);
            return  await retryableFunc.ExecuteAsync(
                    async () => await PollAmazonSwfForActivityTaskAsync(taskQueue, cancellationToken),
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

        private async Task<ActivityTask> PollAmazonSwfForActivityTaskAsync(TaskQueue taskQueue, CancellationToken cancellationToken)
        {
            var activityTaskPollingRequest = taskQueue.CreateActivityTaskPollingRequest(_name);
            Console.WriteLine("Polling for activity task on queue {0} and on domain {1}", activityTaskPollingRequest.TaskList.Name, activityTaskPollingRequest.Domain);
            var response = await _simpleWorkflowClient.PollForActivityTaskAsync(activityTaskPollingRequest, cancellationToken);
            return response?.ActivityTask;
        }

        internal async Task<DecisionTask> PollForDecisionTaskAsync(TaskQueue taskQueue)
        {
            return await taskQueue.ReadStrategy(this, taskQueue);
        }

        public async Task<DecisionTask> PollForDecisionTaskAsync(TaskQueue taskQueue, string nextPageToken)
        {
            Ensure.NotNull(taskQueue, "taskQueue");
            var retryableFunc = new RetryableFunc(_errorHandler);
            return await retryableFunc.ExecuteAsync(
                    async () => await PollAmazonSwfForDecisionTaskAsync(taskQueue, nextPageToken),
                    EmptyDecisionTask);
        }

        private async Task<DecisionTask> PollAmazonSwfForDecisionTaskAsync(TaskQueue taskQueue, string nextPageToken)
        {
            var request = taskQueue.CreateDecisionTaskPollingRequest(_name, nextPageToken);
            var response = await _simpleWorkflowClient.PollForDecisionTaskAsync(request);
            return response?.DecisionTask;
        } 

        public WorkflowsHost Host(IEnumerable<Workflow> workflows)
        {
            return new WorkflowsHost(this,workflows);
        }
        /// <summary>
        /// Create host for activities.
        /// </summary>
        /// <param name="activitiesTypes">List of activities to host.</param>
        /// <param name="instanceCreator">Factory to create activity instances.</param>
        /// <returns></returns>
        public ActivitiesHost Host(IEnumerable<Type> activitiesTypes, Func<Type, Activity> instanceCreator)
        {
            return new ActivitiesHost(this, activitiesTypes, instanceCreator);
        }
        /// <summary>
        /// Create the host for activities. It expect activity to have parameterless constructure.
        /// </summary>
        /// <param name="activitiesTypes">List of activities to host.</param>
        /// <returns></returns>
        public ActivitiesHost Host(IEnumerable<Type> activitiesTypes)
        {
            return new ActivitiesHost(this, activitiesTypes);
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