using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public abstract class SchedulableItem
    {
        protected readonly HashSet<SchedulableItem> ParentItems = new HashSet<SchedulableItem>();
        protected readonly string Name;
        protected readonly string Version;
        protected readonly string PositionalName;

        protected SchedulableItem(string name, string verison, string positionalName)
        {
            Name = name;
            Version = verison;
            PositionalName = positionalName;
        }

        internal bool HasNoParents()
        {
            return ParentItems.Count == 0;
        }

        internal bool IsChildOf(SchedulableItem schedulableItem)
        {
            return ParentItems.Contains(schedulableItem);
        }

        internal abstract Decision GetDecision();

        public bool Has(string name, string version, string positionalName)
        {
            return string.Equals(Name, name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Version, version, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PositionalName, positionalName);
        }

        public override bool Equals(object other)
        {
            var otherItem = other as SchedulableItem;
            if (otherItem == null)
                return false;

            return Has(otherItem.Name, otherItem.Version, otherItem.PositionalName);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}{1}{2}", Name, Version, PositionalName).GetHashCode();
        }

        public bool AllParentsAreProcessed(IWorkflowContext workflowContext)
        {
            return ParentItems.All(p => p.IsProcessed(workflowContext));
        }

        protected abstract bool IsProcessed(IWorkflowContext workflowContext);
    }
}
