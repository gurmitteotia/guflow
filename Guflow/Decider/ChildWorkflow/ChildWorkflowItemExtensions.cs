// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Decider
{
    public static class ChildWorkflowItemExtensions
    {
        /// <summary>
        /// Access completed result of child workflow as dynamic object. If completed result is JSON object then you can directly access its properties.
        /// Throws exception when last event is not <see cref="ChildWorkflowCompletedEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static dynamic Result(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            var completedEvent = item.LastEvent() as ChildWorkflowCompletedEvent;
            if (completedEvent == null)
                throw new InvalidOperationException(Resources.ChildWorkflow_result_can_not_be_accessed);
            return completedEvent.Result();
        }

        /// <summary>
        /// Access completed result of child workflow as TType object. 
        /// Throws exception when last event is not <see cref="ChildWorkflowCompletedEvent"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TType Result<TType>(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            var completedEvent = item.LastEvent() as ChildWorkflowCompletedEvent;
            if (completedEvent == null)
                throw new InvalidOperationException(Resources.ChildWorkflow_result_can_not_be_accessed);

            return completedEvent.Result<TType>();
        }

        /// <summary>
        /// Returns true if the last event of child workflow is <see cref="ChildWorkflowCompletedEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasCompleted(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() is ChildWorkflowCompletedEvent;
        }


        /// <summary>
        /// Returns true if the last event of child workflow is <see cref="ChildWorkflowFailedEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasFailed(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() is ChildWorkflowFailedEvent;
        }

        /// <summary>
        /// Returns true if the last event of child workflow is <see cref="ChildWorkflowTimedoutEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasTimedout(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() is ChildWorkflowTimedoutEvent;
        }
        /// <summary>
        /// Returns true if last event of child workflow is <see cref="ChildWorkflowTerminatedEvent"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasTerminated(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() is ChildWorkflowTerminatedEvent;
        }
        /// <summary>
        /// Returns true if last event of child workflow is <see cref="ChildWorkflowCancelledEvent"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasCancelled(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() is ChildWorkflowCancelledEvent;
        }
        internal static IChildWorkflowItem First(this IEnumerable<IChildWorkflowItem> items, string name, string version,string positionalName = "")
        {
            return items.OfType<ChildWorkflowItem>().First(t => t.Has(Identity.New(name,version ,positionalName)));
        }
        internal static IChildWorkflowItem First<TWorkflow>(this IEnumerable<IChildWorkflowItem> items, string positionalName = "") where TWorkflow :Workflow
        {
            var desc = WorkflowDescription.FindOn<TWorkflow>();
            return items.First(desc.Name, desc.Version, positionalName);
        }
    }
}