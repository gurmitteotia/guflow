// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    internal interface IWorkflowClosingActions
    {
        WorkflowAction OnCompletion(string result, bool proposal);

        WorkflowAction OnFailure(string reason, string detail);

        WorkflowAction OnCancellation(string detail);
    }
}