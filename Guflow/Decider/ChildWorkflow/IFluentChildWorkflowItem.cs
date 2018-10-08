﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    public interface IFluentChildWorkflowItem : IFluentWorkflowItem<IFluentChildWorkflowItem>
    {
        /// <summary>
        /// Configure the handler to provide the input for child workflow. A complex object is serialized to JSON format while primitive types are converted to string
        /// when sending to Amazon SWF.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem WithInput(Func<IChildWorkflowItem, object> input);

        /// <summary>
        /// Configure the handler to provide child policy.
        /// </summary>
        /// <param name="childPolicy"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem WithChildPolicy(Func<IChildWorkflowItem, string> childPolicy);

        /// <summary>
        /// Configure the priority for scheduling the child workflow. High number means high priority.
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem WithPriority(Func<IChildWorkflowItem, int?> priority);

        /// <summary>
        /// Configure the tasklist child workflow should be scheduled on.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnTaskList(Func<IChildWorkflowItem, string> name);


        /// <summary>
        /// Configure the lambda role for workflow. This lambda role will be used by child workflow to schedule aws lambda functions.
        /// </summary>
        /// <param name="lambdaRole"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem WithLambdaRole(Func<IChildWorkflowItem, string> lambdaRole);

        /// <summary>
        /// Configure the execution timeouts for child workflow.
        /// </summary>
        /// <param name="timeouts"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem WithTimeouts(Func<IChildWorkflowItem, WorkflowTimeouts> timeouts);

        /// <summary>
        /// Provide tags when scheduling the child workflow.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem WithTags(Func<IChildWorkflowItem, IEnumerable<string>> tags);

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