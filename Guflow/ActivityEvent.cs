using System;

namespace Guflow
{
    public abstract class ActivityEvent : WorkflowEvent
    {
        public string Name { get; protected set; }
        public string Version { get; protected set; }
        public string PositionalName { get; protected set; }

        public bool Has(string name, string version, string positionalName)
        {
            return string.Equals(Name, name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Version, version, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PositionalName, positionalName);
        }
    }
}