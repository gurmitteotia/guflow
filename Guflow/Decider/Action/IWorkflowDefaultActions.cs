// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    internal interface IWorkflowDefaultActions
    {
        WorkflowAction Continue(WorkflowItemEvent workflowItemEvent);
        WorkflowAction StartWorkflow();
        WorkflowAction FailWorkflow(string reason, string details);
        WorkflowAction CancelWorkflow(string details);
        WorkflowAction Ignore();
        WorkflowAction ResumeOnSignal(string signalName);
    }
}