using System;
using System.Threading.Tasks;
using Amazon;
using Guflow.Decider;
using Guflow.Worker;

namespace Guflow.IntegrationTests
{
    public  class TestDomain
    {
        private const string DomainName = "GuflowTestDomain";
        private readonly Domain _domain;

        public TestDomain()
        {
            _domain = new Domain(DomainName, RegionEndpoint.EUWest1);
        }

        public async Task<HostedWorkflows> Host(params Workflow[] workflows)
        {
            await _domain.RegisterAsync(2);
            foreach (var workflow in workflows)
            {
                await _domain.RegisterWorkflowAsync(workflow.GetType());
            }
            
            return _domain.Host(workflows);
        }

        public async Task<HostedActivities> Host(Type[] activityTypes)
        {
            await _domain.RegisterAsync(2);
            foreach (var activityType in activityTypes)
            {
                await _domain.RegisterActivityAsync(activityType);
            }
            return _domain.Host(activityTypes);
        }

        public async Task<string> StartWorkflow<TWorkflow>(string input, string taskListName) where TWorkflow :Workflow
        {
            var workflowId = Guid.NewGuid().ToString();
            var startRequest = StartWorkflowRequest.For<TWorkflow>(workflowId);
            startRequest.TaskListName = taskListName;
            startRequest.Input = input;
            await _domain.StartWorkflowAsync(startRequest);
            return workflowId;
        }

        public async Task SendSignal(string workflowId, string name, string input)
        {
            await _domain.SignalWorkflowAsync(new SignalWorkflowRequest(workflowId, name){ SignalInput = input });
        }
    }
}