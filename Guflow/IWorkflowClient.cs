using System;
using System.Threading.Tasks;

namespace Guflow
{
    public interface IWorkflowClient
    {
        Task Register<T>() where T: Workflow;
        Task Register(WorkflowDescription workflowDescription);
        Task Register(Type workflowType);
        WorkflowHost CreateHostFor(Workflow workflow);
    }
}