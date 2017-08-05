﻿using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public static class TimerItemsExtension
    {
        public static ITimerItem First(this IEnumerable<ITimerItem> timerItems, string name)
        {
            Ensure.NotNull(timerItems, "timerItems");
            return timerItems.OfType<TimerItem>().First(t => t.Has(Identity.Timer(name)));
        }
    }
}