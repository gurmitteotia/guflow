using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public interface IWorkflowClient
    {
        Task Register<T>() where T: Workflow;
        Task Register(WorkflowDescription workflowDescription);
        Task Register(Type workflowType);
        Task RespondWithDecisions(string taskToken, IEnumerable<Decision> decisions);
    }
}