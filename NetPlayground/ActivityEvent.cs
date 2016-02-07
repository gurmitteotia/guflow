using System;

namespace NetPlayground
{
    public abstract class ActivityEvent : WorkflowEvent
    {
        public string Name { get; protected set; }
        public string Version { get; protected set; }
        public string PositionalName { get; protected set; }
        public abstract bool IsProcessed { get; }

        public bool Has(string name, string version, string positionalName)
        {
            return string.Equals(Name, name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Version, version, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PositionalName, positionalName);
        }
    }
}