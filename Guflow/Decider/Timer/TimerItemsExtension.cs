// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public static class TimerItemsExtension
    {
        /// <summary>
        /// Returns the first scheduled timer object for the given name.
        /// </summary>
        /// <param name="timerItems"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ITimerItem First(this IEnumerable<ITimerItem> timerItems, string name)
        {
            Ensure.NotNull(timerItems, "timerItems");
            return timerItems.OfType<TimerItem>().First(t => t.Has(Identity.Timer(name)));
        }

        /// <summary>
        /// Returns true if last event of timer is fired event.
        /// </summary>
        /// <param name="timerItem"></param>
        /// <returns></returns>
        public static bool IsFired(this ITimerItem timerItem) => timerItem.LastEvent() is TimerFiredEvent;

        /// <summary>
        /// Returns true if last of event of timer is cancelled event.
        /// </summary>
        /// <param name="timerItem"></param>
        /// <returns></returns>
        public static bool IsCancelled(this ITimerItem timerItem) => timerItem.LastEvent() is TimerCancelledEvent;

    }
}