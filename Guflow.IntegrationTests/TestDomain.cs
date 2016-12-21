using System;
using System.Threading.Tasks;
using Amazon;
using Guflow.Decider;

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

        public async Task StartWorkflow<TWorkflow>(string input, string taskListName) where TWorkflow :Workflow
        {
            var startRequest = StartWorkflowRequest.For<TWorkflow>(Guid.NewGuid().ToString());
            startRequest.TaskListName = taskListName;
            startRequest.Input = input;
            await _domain.StartWorkflowAsync(startRequest);
        }
    }
}