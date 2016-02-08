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

        public static IEnumerable<SchedulableItem> GetChildernOf(this HashSet<SchedulableItem> schedulableItems, SchedulableItem item)
        {
            return schedulableItems.Where(s => s.IsChildOf(item));
        }

        public static SchedulableItem Find(this HashSet<SchedulableItem> schedulableItems, string name, string version, string positionalName)
        {
            return schedulableItems.FirstOrDefault(s => s.Has(name,version,positionalName));
        }

        public static ActivityItem FindActivity(this HashSet<SchedulableItem> schedulableItems, string name, string version, string positionalName)
        {
            return schedulableItems.FirstOrDefault(s => s.Has(name, version, positionalName)) as ActivityItem;
        }
    }
}