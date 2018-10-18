// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    /// <summary>
    /// Fluent API interface to schedule the child workflow.
    /// </summary>
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
        /// Configure the handler to return the <see cref="WorkflowAction"/> on <see cref="ChildWorkflowCompletedEvent"/>.
        /// </summary>
        /// <param name="workflowAction">Handler</param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnCompletion(Func<ChildWorkflowCompletedEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the <see cref="WorkflowAction"/> on <see cref="ChildWorkflowFailedEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnFailure(Func<ChildWorkflowFailedEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the <see cref="WorkflowAction"/> on <see cref="ChildWorkflowCancelledEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnCancelled(Func<ChildWorkflowCancelledEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the <see cref="WorkflowAction"/> on <see cref="ChildWorkflowTerminatedEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnTerminated(Func<ChildWorkflowTerminatedEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the <see cref="WorkflowAction"/> on <see cref="ChildWorkflowTimedoutEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnTimedout(Func<ChildWorkflowTimedoutEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Configure the handler to return the <see cref="WorkflowAction"/> on <see cref="ChildWorkflowStartFailedEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnStartFailed(Func<ChildWorkflowStartFailedEvent, WorkflowAction> workflowAction);

        /// <summary>
        /// Schedule child workflow only when provided expression is evaluated to true. If it is evaluated to false the it will trigger the scheduling of first joint item,
        /// however you can change this behaviour by other overloaded "When" method.
        /// </summary>
        /// <param name="true"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem When(Func<IChildWorkflowItem, bool> @true);

        /// <summary>
        /// Schedule child workflow only when provided express is evaluated to true. You can also provide custom workflow action when this condition is evaulated to false.
        /// Please refer to Deflow algorithm for more details.
        /// </summary>
        /// <param name="true"></param>
        /// <param name="falseAction">Provide WorkflowAction when scheduling condition is evaluated to false. By default it will try to schedule the first join item if this is a non startup item.</param>
        /// <returns></returns>
        IFluentChildWorkflowItem When(Func<IChildWorkflowItem, bool> @true, Func<IChildWorkflowItem, WorkflowAction> falseAction);

        /// <summary>
        /// Configure the handler to return the <see cref="WorkflowAction"/> on <see cref="ExternalWorkflowCancelRequestFailedEvent"/>.
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <returns></returns>
        IFluentChildWorkflowItem OnCancellationFailed(Func<ExternalWorkflowCancelRequestFailedEvent, WorkflowAction> workflowAction);
    }
}