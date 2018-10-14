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
        /// <param name="lambdaItem"></param>
        /// <returns></returns>
        public static dynamic Result(this ILambdaItem lambdaItem)
        {
            Ensure.NotNull(lambdaItem, nameof(lambdaItem));
            var completedEvent = lambdaItem.LastEvent() as LambdaCompletedEvent;
            if (completedEvent == null)
                throw new InvalidOperationException(Resources.Lambda_result_can_not_be_accessed);
            return completedEvent.Result();
        }

        /// <summary>
        /// Access completed result of lambda as TType object. 
        /// Throws exception when last event is not lambda completed event.
        /// </summary>
        /// <param name="lambdaItem"></param>
        /// <returns></returns>
        public static TType Result<TType>(this ILambdaItem lambdaItem)
        {
            Ensure.NotNull(lambdaItem, nameof(lambdaItem));
            var completedEvent = lambdaItem.LastEvent() as LambdaCompletedEvent;
            if (completedEvent == null)
                throw new InvalidOperationException(Resources.Lambda_result_can_not_be_accessed);

            return completedEvent.Result<TType>();
        }

        /// <summary>
        /// Returns true if the last event of lambda is <see cref="LambdaCompletedEvent"/>.
        /// </summary>
        /// <param name="lambdaItem"></param>
        /// <returns></returns>
        public static bool HasCompleted(this ILambdaItem lambdaItem)
        {
            Ensure.NotNull(lambdaItem, nameof(lambdaItem));
            return lambdaItem.LastEvent() is LambdaCompletedEvent;
        }


        /// <summary>
        /// Returns true if the last event of lambda is <see cref="LambdaFailedEvent"/>.
        /// </summary>
        /// <param name="lambdaItem"></param>
        /// <returns></returns>
        public static bool HasFailed(this ILambdaItem lambdaItem)
        {
            Ensure.NotNull(lambdaItem, nameof(lambdaItem));
            return lambdaItem.LastEvent() is LambdaFailedEvent;
        }

        /// <summary>
        /// Returns true if the last event of lambda is <see cref="LambdaTimedoutEvent"/>.
        /// </summary>
        /// <param name="lambdaItem"></param>
        /// <returns></returns>
        public static bool HasTimedout(this ILambdaItem lambdaItem)
        {
            Ensure.NotNull(lambdaItem, nameof(lambdaItem));
            return lambdaItem.LastEvent() is LambdaTimedoutEvent;
        }

        internal static ILambdaItem First(this IEnumerable<ILambdaItem> items, string name, string positionalName = "")
        {
            Ensure.NotNull(items, nameof(items));
            return items.OfType<LambdaItem>().First(t => t.Has(Identity.Lambda(name, positionalName)));
        }
        
    }
}