using System.Collections.Generic;
using System.Linq;

namespace NetPlayground
{
    public static class SchedulableItemExtensions
    {
        public static IEnumerable<SchedulableItem> GetStartupItems(this HashSet<SchedulableItem> scheudleItems)
        {
            return scheudleItems.Where(s => s.HasNoParents());
        }

        public static IEnumerable<SchedulableItem> GetChildernOf(this HashSet<SchedulableItem> schedulableItems, ActivityName activityName)
        {
            return schedulableItems.Where(s => s.IsParent(activityName));
        }
    }
}