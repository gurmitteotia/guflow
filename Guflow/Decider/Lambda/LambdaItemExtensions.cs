// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Decider
{
    public static class LambdaItemExtensions
    {
        /// <summary>
        /// Access completed result of lambda function as dynamic object. If completed result is JSON object then you can directly access its properties.
        /// Throws exception when last event is not lambda completed event.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static dynamic Result(this ILambdaItem item)
        {
            Ensure.NotNull(item, nameof(item));
            var completedEvent = item.LastEvent() as LambdaCompletedEvent;
            if (completedEvent == null)
                throw new InvalidOperationException(Resources.Lambda_result_can_not_be_accessed);
            return completedEvent.Result();
        }

        /// <summary>
        /// Access completed result of lambda as TType object. 
        /// Throws exception when last event is not lambda completed event.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TType Result<TType>(this ILambdaItem item)
        {
            Ensure.NotNull(item, nameof(item));
            var completedEvent = item.LastEvent() as LambdaCompletedEvent;
            if (completedEvent == null)
                throw new InvalidOperationException(Resources.Lambda_result_can_not_be_accessed);

            return completedEvent.Result<TType>();
        }

        /// <summary>
        /// Returns true if the last event of lambda is <see cref="LambdaCompletedEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasCompleted(this ILambdaItem item)
            => item.LastCompletedEvent() != null;



        /// <summary>
        /// Returns true if the last event of lambda is <see cref="LambdaFailedEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasFailed(this ILambdaItem item)
            => item.LastFailedEvent() != null;


        /// <summary>
        /// Returns true if the last event of lambda is <see cref="LambdaTimedoutEvent"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool HasTimedout(this ILambdaItem item) => item.LastTimedoutEvent() != null;
      

        /// <summary>
        /// Retruns the <see cref="LambdaFailedEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <returns></returns>
        public static LambdaFailedEvent LastFailedEvent(this ILambdaItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as LambdaFailedEvent;
        }
        /// <summary>
        ///  Retruns the <see cref="LambdaTimedoutEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static LambdaTimedoutEvent LastTimedoutEvent(this ILambdaItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as LambdaTimedoutEvent;
        }

        /// <summary>
        /// Retruns the <see cref="LambdaCompletedEvent"/> and if it is the last event, otherwise null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static LambdaCompletedEvent LastCompletedEvent(this ILambdaItem item)
        {
            Ensure.NotNull(item, nameof(item));
            return item.LastEvent() as LambdaCompletedEvent;
        }

        internal static ILambdaItem First(this IEnumerable<ILambdaItem> items, string name, string positionalName = "")
        {
            Ensure.NotNull(items, nameof(items));
            return items.OfType<LambdaItem>().First(t => t.Has(Identity.Lambda(name, positionalName)));
        }
        
    }
}