// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;
using Guflow.Worker;

namespace Guflow.Decider
{
    public static class ActivityItemExtension
    {
        /// <summary>
        /// Access completed result of activity as dynamic object. If completed result is JSON object then you can directly access its properties.
        /// Throws exception when last event is not activity completed event.
        /// </summary>
        /// <param name="activityItem"></param>
        /// <returns></returns>
        public static dynamic Result(this IActivityItem activityItem)
        {
            Ensure.NotNull(activityItem, "activityItem");
            var completedEvent = activityItem.LastEvent();
            var activityCompletedEvent = completedEvent as ActivityCompletedEvent;
            if(activityCompletedEvent == null)
                throw new InvalidOperationException(string.Format(Resources.Activity_result_can_not_accessed,
                                                    typeof(ActivityCompletedEvent), completedEvent!=null? completedEvent.GetType().ToString(): "Unkown"));
            return activityCompletedEvent.Result();
        }

        /// <summary>
        /// Access completed result of activity as TType object. 
        /// Throws exception when last event is not activity completed event.
        /// </summary>
        /// <param name="activityItem"></param>
        /// <returns></returns>
        public static TType Result<TType>(this IActivityItem activityItem)
        {
            Ensure.NotNull(activityItem, "activityItem");
            var completedEvent = activityItem.LastEvent();
            var activityCompletedEvent = completedEvent as ActivityCompletedEvent;
            if (activityCompletedEvent == null)
                throw new InvalidOperationException(string.Format(Resources.Activity_result_can_not_accessed,
                                                    typeof(ActivityCompletedEvent), completedEvent != null ? completedEvent.GetType().ToString() : "Unkown"));
            return activityCompletedEvent.Result<TType>();
        }

        /// <summary>
        /// Returns true if the last event of activity is <see cref="ActivityCompletedEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasCompleted(this IActivityItem item) => item.LastCompletedEvent() != null;

        /// <summary>
        /// Returns true if the last event of activity is <see cref="ActivityFailedEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasFailed(this IActivityItem item) => item.LastFailedEvent() != null;

        /// <summary>
        /// Returns true if the last event of activity is <see cref="ActivityTimedoutEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasTimedout(this IActivityItem item) => item.LastTimedoutEvent() != null;

        /// <summary>
        /// Returns true if the last event of activity is <see cref="ActivityCancelledEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasCancelled(this IActivityItem item) => item.LastCancelledEvent() != null;


        /// <summary>
        /// Retruns the <see cref="ActivityFailedEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ActivityFailedEvent LastFailedEvent(this IActivityItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ActivityFailedEvent;
        }
        /// <summary>
        ///  Retruns the <see cref="ActivityTimedoutEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ActivityTimedoutEvent LastTimedoutEvent(this IActivityItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ActivityTimedoutEvent;
        }
        /// <summary>
        /// Retruns the <see cref="ActivityCancelledEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ActivityCancelledEvent LastCancelledEvent(this IActivityItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ActivityCancelledEvent;
        }

        /// <summary>
        /// Retruns the <see cref="ActivityCompletedEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ActivityCompletedEvent LastCompletedEvent(this IActivityItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as ActivityCompletedEvent;
        }

        internal static IActivityItem First(this IEnumerable<IActivityItem> activityItems, string name, string version, string positionalName = "")
        {
            var identity = Identity.New(name, version, positionalName);
            return activityItems.OfType<ActivityItem>().First(a => a.Has(identity));
        }
        internal static IActivityItem First<TActivity>(this IEnumerable<IActivityItem> activityItems, string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            return activityItems.First(description.Name, description.Version, positionalName);
        }
    }
}