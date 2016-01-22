using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public abstract class SchedulableItem
    {
        private readonly HashSet<ActivityName> _parentActivities = new HashSet<ActivityName>();

        internal bool HasNoParents()
        {
            return _parentActivities.Count == 0;
        }

        internal bool IsParent(ActivityName name)
        {
            return _parentActivities.Contains(name);
        }

        internal abstract Decision GetDecision();

        public SchedulableItem DependsOn(string name, string version, string positionalName = "")
        {
            _parentActivities.Add(new ActivityName(name, version, positionalName));

            return this;
        }
    }
}
