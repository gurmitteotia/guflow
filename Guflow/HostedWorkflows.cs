using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guflow.Properties;

namespace Guflow
{
    public sealed class HostedWorkflows
    {
        private readonly Domain _domain;
        private readonly Dictionary<string, Workflow> _hostedWorkflows = new Dictionary<string, Workflow>();
        private volatile bool _cancelled = false;
        internal Func<bool> Cancelled; 
        public HostedWorkflows(Domain domain, IEnumerable<Workflow> workflows)
        {
            _domain = domain;
            Cancelled = ()=>_cancelled;
            PopulateHostedWorkflows(workflows);
        }

        internal Workflow FindBy(string name, string version)
        {
            Workflow hostedWorkflow;
            var hostedWorkflowKey = name + version;
            if(!_hostedWorkflows.TryGetValue(hostedWorkflowKey, out hostedWorkflow))
                throw new WorkflowNotHostedException(string.Format(Resources.Workflow_not_hosted,name,version));
            return hostedWorkflow;
        }

        private void PopulateHostedWorkflows(IEnumerable<Workflow> workflows)
        {
            foreach (var workflow in workflows)
            {
                var workflowDescription = WorkflowDescriptionAttribute.FindOn(workflow.GetType());
                var hostedWorkflowKey = workflowDescription.Name + workflowDescription.Version;
                if(_hostedWorkflows.ContainsKey(hostedWorkflowKey))
                    throw new WorkflowAlreadyHostedException(string.Format(Resources.Workflow_already_hosted, workflowDescription.Name,workflowDescription.Version));
                _hostedWorkflows.Add(hostedWorkflowKey,workflow);
            }
        }

        public async Task StartExecutionAsync(TaskQueue taskQueue)
        {
            while (!Cancelled())
            {
                var workflowTask = await taskQueue.PollForNewTasksAsync(_domain);
                await workflowTask.ExecuteForAsync(this);
            }
        }
    }
}