// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;

namespace Guflow.Decider
{
    public interface IFluentChildWorkflowItem : IFluentWorkflowItem<IFluentChildWorkflowItem>
    {
        /// <summary>
        /// Configure the handler to return the workflow action on <see cref="ChildWorkflowCompletedEvent"/>.
        /// </summary>
        /// <param name="workflowAction">Handler</param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnCompletion(Func<ChildWorkflowCompletedEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the workflow action on <see cref="ChildWorkflowFailedEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnFailure(Func<ChildWorkflowFailedEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the workflow action on <see cref="ChildWorkflowCancelledEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnCancelled(Func<ChildWorkflowCancelledEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the workflow on <see cref="ChildWorkflowTerminatedEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnTerminated(Func<ChildWorkflowTerminatedEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the workflow on <see cref="ChildWorkflowTimedoutEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnTimedout(Func<ChildWorkflowTimedoutEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the workflow on <see cref="ChildWorkflowStartFailedEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnStartFailed(Func<ChildWorkflowStartFailedEvent, WorkflowAction> workflowAction);
    }
}