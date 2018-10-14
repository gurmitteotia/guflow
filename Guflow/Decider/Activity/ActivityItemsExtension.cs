// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal static class ActivityItemsExtension
    {
        internal static IActivityItem First(this IEnumerable<IActivityItem> activityItems, string name, string version, string positionalName = "")
        {
            var identity = Identity.New(name, version, positionalName);
            return activityItems.OfType<ActivityItem>().First(a => a.Has(identity));
        }
        internal static IActivityItem First<TActivity>(this IEnumerable<IActivityItem> activityItems, string positionalName = "") where TActivity: Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            return activityItems.First(description.Name, description.Version, positionalName);
        }
    }
}