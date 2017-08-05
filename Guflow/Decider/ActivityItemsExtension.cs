using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    public static class ActivityItemsExtension
    {
        public static IActivityItem First(this IEnumerable<IActivityItem> activityItems, string name, string version, string positionalName = "")
        {
            var identity = Identity.New(name, version, positionalName);
            return activityItems.OfType<ActivityItem>().First(a => a.Has(identity));
        }

        public static IActivityItem First<TActivity>(this IEnumerable<IActivityItem> activityItems, string positionalName = "") where TActivity: Activity
        {
            var description = ActivityDescriptionAttribute.FindOn<TActivity>();
            return activityItems.First(description.Name, description.Version, positionalName);
        }
    }
}