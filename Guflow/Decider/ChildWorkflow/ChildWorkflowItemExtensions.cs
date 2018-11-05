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
        public static bool HasCompleted(this IChildWorkflowItem item) => item.LastCompletedEvent() != null;



        /// <summary>
        /// Returns true if the last event of child workflow is <see cref="ChildWorkflowFailedEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasFailed(this IChildWorkflowItem item) => item.LastFailedEvent() != null;

        /// <summary>
        /// Returns true if the last event of child workflow is <see cref="ChildWorkflowTimedoutEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasTimedout(this IChildWorkflowItem item) => item.LastTimedoutEvent() != null;

        /// <summary>
        /// Returns true if last event of child workflow is <see cref="ChildWorkflowTerminatedEvent"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasTerminated(this IChildWorkflowItem item) => item.LastTerminatedEvent() != null;
      
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
        /// <summary>
        /// Retruns the <see cref="ChildWorkflowFailedEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ChildWorkflowFailedEvent LastFailedEvent(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ChildWorkflowFailedEvent;
        }
        /// <summary>
        ///  Retruns the <see cref="ChildWorkflowTimedoutEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ChildWorkflowTimedoutEvent LastTimedoutEvent(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ChildWorkflowTimedoutEvent;
        }
        /// <summary>
        /// Retruns the <see cref="ChildWorkflowCancelledEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ChildWorkflowCancelledEvent LastCancelledEvent(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ChildWorkflowCancelledEvent;
        }

        /// <summary>
        /// Retruns the <see cref="ChildWorkflowCompletedEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ChildWorkflowCompletedEvent LastCompletedEvent(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ChildWorkflowCompletedEvent;
        }

        /// <summary>
        /// Retruns the <see cref="ChildWorkflowTerminatedEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ChildWorkflowTerminatedEvent LastTerminatedEvent(this IChildWorkflowItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ChildWorkflowTerminatedEvent;
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